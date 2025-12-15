using System.Text;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Logging;

namespace ViolastroBot.Features.MessageStrategies;

public sealed partial class OffensiveWordChecker : IMessageStrategy, ISingleton
{
    [method: JsonConstructor]
    private sealed class OffensiveWords(string[] nWords, char[] nVariants)
    {
        public string[] NWords { get; } = nWords;
        public char[] NVariants { get; } = nVariants;
    }

    private const ulong OffensiveWordsMessageId = 1148574093948489810;

    private readonly ILoggingService _logger;
    private readonly DiscordSocketClient _client;

    private static OffensiveWords? _offensiveWords;

    public OffensiveWordChecker(DiscordSocketClient client, ILoggingService logger)
    {
        _logger = logger;
        _client = client;
        _client.Ready += FetchOffensiveWordsFromPrivateChannel;
    }

    public async Task<bool> ExecuteAsync(SocketUserMessage message)
    {
        if (_offensiveWords == null)
        {
            return false;
        }

        var messageContent = message.Content;
        var sanitizedContent = SanitizeContent(messageContent);

        if (!IsOffensive(sanitizedContent, out var detectedWord))
        {
            return false;
        }

        if (IsUrl(messageContent))
        {
            return false;
        }

        await TakeActionOnOffensiveMessageAsync(message, detectedWord);

        return true;
    }

    private async Task FetchOffensiveWordsFromPrivateChannel()
    {
        if (_client.GetChannel(Channels.OffensiveWords) is not SocketTextChannel channel)
        {
            Console.WriteLine("Failed to fetch the offensive words channel.");
            return;
        }

        if (await channel.GetMessageAsync(OffensiveWordsMessageId) is not IUserMessage message)
        {
            Console.WriteLine("Failed to fetch the offensive words message.");
            return;
        }

        var content = message.Content;

        try
        {
            _offensiveWords = JsonConvert.DeserializeObject<OffensiveWords>(content);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to parse the offensive words message: {ex.Message}");
        }
    }

    private async Task TakeActionOnOffensiveMessageAsync(SocketUserMessage message, string detectedWord)
    {
        await message.DeleteAsync();
        await message.Author.SendMessageAsync(
            $"Please do not use offensive words on our Discord server, as stated in the rules.{Environment.NewLine}If you think this was a mistake, please ignore this warning and contact a moderator."
        );
        await message.Channel.SendMessageAsync("Y'all best not be actin' offensive!");

        var guild = (message.Channel as SocketGuildChannel)?.Guild;
        var role = guild?.GetRole(Roles.Moderator);

        if (guild != null && role != null)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Attention {role.Mention}, a potentially offensive message has been detected.");
            sb.AppendLine($"Message content: \"{message.Content}\"");
            sb.AppendLine($"Message author: {message.Author.Mention}.");
            sb.AppendLine($"Message channel: <#{message.Channel.Id}>.");
            sb.AppendLine($"Detected word: \"{detectedWord}\"");

            await _logger.LogMessageAsync(sb.ToString());
        }
    }

    private static bool IsOffensive(string sanitizedContent, out string? detectedWord)
    {
        if (_offensiveWords is null)
        {
            detectedWord = null;
            return false;
        }

        foreach (var word in _offensiveWords.NWords)
        {
            if (!sanitizedContent.Contains(word))
            {
                continue;
            }

            var strIndex = sanitizedContent.IndexOf(word, StringComparison.Ordinal) - 1;

            if (!IsNVariant(sanitizedContent, strIndex))
            {
                continue;
            }

            detectedWord = word;

            return true;
        }

        detectedWord = null;

        return false;
    }

    private static bool IsNVariant(string sanitizedContent, int strIndex)
    {
        return _offensiveWords.NVariants.Any(nChar => sanitizedContent[strIndex] == nChar);
    }

    private static string SanitizeContent(string content)
    {
        return MultipleSpacesRegex().Replace(content, " ");
    }

    private static bool IsUrl(string content)
    {
        return UrlRegex().IsMatch(content);
    }

    [GeneratedRegex("\\s+")]
    private static partial Regex MultipleSpacesRegex();

    [GeneratedRegex("[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b([-a-zA-Z0-9()@:%_\\+.~#?&//=]*)?")]
    private static partial Regex UrlRegex();
}
