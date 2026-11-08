using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;
using ViolastroBot.Features.Contests;

namespace ViolastroBot.Features.Chat;

public sealed class ViolastroChat(
    DiscordSocketClient client,
    OllamaClient ollama,
    ChatReplyWordFilter wordFilter,
    ContestSubmissions contestSubmissions
) : IStartupTask, ISingleton
{
    private const int MaxPendingReplies = 3;
    private const int MaxReplyTokens = 80;
    private const int MaxHistoryMessages = 3;
    private const int MaxInputLength = 500;
    private const int WarmUpAttempts = 10;
    private const string BusyReply = "Bwuh! One at a time, y'all!";
    private const string UnseenEmoji = "[sent an emoji your screen can't display]";
    private const string UnseenAttachment = "[sent an attachment your screen can't display]";

    private static readonly TimeSpan UserCooldown = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ReplyTimeout = TimeSpan.FromSeconds(45);
    private static readonly TimeSpan WarmUpTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan WarmUpRetryDelay = TimeSpan.FromSeconds(30);

    private static readonly Emote RefusalReaction = Emote.Parse("<:caterpillar:988462693805600828>");
    private static readonly Emoji CooldownReaction = new("⏳");
    private static readonly Emoji FailureReaction = new("💤");

    private static readonly string[] Deflections =
    [
        "Bwehehe! Y'all ain't gettin' that outta me!",
        "Nice try, ya fool!",
        "Not a chance, y'all! Bwehehe!",
    ];

    private readonly ConcurrentDictionary<ulong, DateTimeOffset> _lastRepliedAt = new();
    private readonly SemaphoreSlim _generating = new(1, 1);
    private readonly Random _random = new();
    private int _pendingReplies;

    public Task InitializeAsync()
    {
        _ = Task.Run(WarmUpAsync);

        return Task.CompletedTask;
    }

    public void TryRespond(SocketUserMessage message)
    {
        if (!ShouldRespond(message))
        {
            return;
        }

        _ = TryStartCooldown(message.Author.Id)
            ? Task.Run(() => RespondAsync(message))
            : Task.Run(() => ReactAsync(message, CooldownReaction));
    }

    private bool ShouldRespond(SocketUserMessage message)
    {
        if (
            message.Channel is not SocketGuildChannel
            || contestSubmissions.IsContestChannel(message.Channel.Id)
            || message.Content.StartsWith(CommandHandler.CommandPrefix)
        )
        {
            return false;
        }

        var botId = client.CurrentUser.Id;

        return message.MentionedUsers.Any(user => user.Id == botId) || message.ReferencedMessage?.Author.Id == botId;
    }

    private bool TryStartCooldown(ulong userId)
    {
        var now = DateTimeOffset.UtcNow;

        if (_lastRepliedAt.TryGetValue(userId, out var lastRepliedAt) && now - lastRepliedAt < UserCooldown)
        {
            return false;
        }

        _lastRepliedAt[userId] = now;

        return true;
    }

    private async Task RespondAsync(SocketUserMessage message)
    {
        var isAccepted = Interlocked.Increment(ref _pendingReplies) <= MaxPendingReplies;

        try
        {
            if (!isAccepted)
            {
                await SendAsync(message, BusyReply);
                return;
            }

            using var typing = message.Channel.EnterTypingState();

            await _generating.WaitAsync();

            try
            {
                var reply = await GenerateReplyAsync(message);

                if (reply != null)
                {
                    await SendAsync(message, reply);
                }
            }
            finally
            {
                _generating.Release();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to generate a chat reply: {ex.Message}");
            await ReactAsync(message, FailureReaction);
        }
        finally
        {
            Interlocked.Decrement(ref _pendingReplies);
        }
    }

    private static async Task ReactAsync(SocketUserMessage message, Emoji reaction)
    {
        try
        {
            await message.AddReactionAsync(reaction);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to add a reaction: {ex.Message}");
        }
    }

    private async Task<string?> GenerateReplyAsync(SocketUserMessage message)
    {
        var text = DescribeMessage(message);

        if (text.Length == 0)
        {
            return null;
        }

        var decoration = ViolastroPersona.PickDecoration();
        var history = await BuildHistoryAsync(message);

        using var timeout = new CancellationTokenSource(ReplyTimeout);

        var completion = await ollama.CompleteAsync(
            ViolastroPersona.BuildConversation(history, text, decoration),
            MaxReplyTokens,
            timeout.Token
        );

        if (ChatReplySanitizer.IsRefusal(completion.Text))
        {
            Console.WriteLine($"Chat refused message {message.Id}.");
            await message.AddReactionAsync(RefusalReaction);
            return null;
        }

        var reply = ChatReplySanitizer.Sanitize(completion.Text, completion.WasTruncated);

        if (reply == null || !wordFilter.IsSafeToSend(reply))
        {
            return Deflections[_random.Next(Deflections.Length)];
        }

        return decoration.Action == null ? reply : $"{reply} {decoration.Action}";
    }

    private async Task<List<ChatTurn>> BuildHistoryAsync(SocketUserMessage message)
    {
        List<ChatTurn> history = [];
        IMessage current = message;

        while (history.Count < MaxHistoryMessages && current.Reference?.MessageId.IsSpecified == true)
        {
            if (await message.Channel.GetMessageAsync(current.Reference.MessageId.Value) is not IUserMessage parent)
            {
                break;
            }

            var text = DescribeMessage(parent);

            if (text.Length == 0)
            {
                break;
            }

            var role = parent.Author.Id == client.CurrentUser.Id ? "assistant" : "user";
            history.Insert(0, new ChatTurn(role, text));
            current = parent;
        }

        return history;
    }

    private static string DescribeMessage(IUserMessage message)
    {
        var text = ExtractText(message);

        if (message.Attachments.Count > 0)
        {
            return text.Length == 0 ? UnseenAttachment : $"{text}\n\n{UnseenAttachment}";
        }

        return text.Length == 0 && message.Tags.Any(tag => tag.Type == TagType.Emoji) ? UnseenEmoji : text;
    }

    private static string ExtractText(IUserMessage message)
    {
        var text = message
            .Resolve(
                userHandling: TagHandling.Remove,
                channelHandling: TagHandling.Name,
                roleHandling: TagHandling.Remove,
                everyoneHandling: TagHandling.Remove,
                emojiHandling: TagHandling.Remove
            )
            .Trim();

        return text.Length > MaxInputLength ? text[..MaxInputLength] : text;
    }

    private static Task SendAsync(SocketUserMessage message, string text)
    {
        return message.ReplyAsync(text, allowedMentions: AllowedMentions.None);
    }

    private async Task WarmUpAsync()
    {
        for (var attempt = 1; attempt <= WarmUpAttempts; attempt++)
        {
            await _generating.WaitAsync();

            try
            {
                using var timeout = new CancellationTokenSource(WarmUpTimeout);
                var conversation = ViolastroPersona.BuildConversation([], "hi", ViolastroPersona.PickDecoration());
                await ollama.CompleteAsync(conversation, 1, timeout.Token);
                return;
            }
            catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
            {
                Console.WriteLine($"Chat warm-up attempt {attempt} failed: {ex.Message}");
            }
            finally
            {
                _generating.Release();
            }

            await Task.Delay(WarmUpRetryDelay);
        }
    }
}
