using Discord;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Logging;

namespace ViolastroBot.Features.Commands.Contests;

public sealed class ContestSubmission(ILoggingService logger, ContestChannelSettings settings) : ISingleton
{
    private readonly HashSet<ulong> _contestChannelIds = settings.ContestChannelIds.ToHashSet();
    private readonly SemaphoreSlim _processingLock = new(1, 1);

    public bool IsContestChannel(ulong channelId) => _contestChannelIds.Contains(channelId);

    public async Task<bool> ProcessAsync(SocketUserMessage message)
    {
        if (message.Channel is not SocketTextChannel)
        {
            return false;
        }

        await _processingLock.WaitAsync();
        try
        {
            await message.Author.SendMessageAsync(
                "Appreciate y'all for submitting a contest entry! I'm checkin' to see if y'all already submitted a message..."
            );

            return await CheckForExistingSubmissionAsync(message);
        }
        catch (Exception ex)
        {
            await LogErrorAndNotifyUserAsync(ex.Message, message);
            return false;
        }
        finally
        {
            _processingLock.Release();
        }
    }

    private static async Task<bool> CheckForExistingSubmissionAsync(SocketUserMessage message)
    {
        var lastMessageId = message.Id;

        if (message.Channel is not SocketTextChannel channel)
        {
            return false;
        }

        while (true)
        {
            var messages = await channel.GetMessagesAsync(lastMessageId, Direction.Before).FlattenAsync();
            var messageList = messages.ToList();

            if (!messageList.Any())
                break;

            var existingMessage = messageList.FirstOrDefault(msg => msg.Author.Id == message.Author.Id);

            if (existingMessage != null)
            {
                await NotifyUserOfDuplicateSubmissionAsync(message, existingMessage);
                return false;
            }

            lastMessageId = messageList.Last().Id;
        }

        await message.AddReactionAsync(new Emoji("👍"));
        return true;
    }

    private async Task LogErrorAndNotifyUserAsync(string errorMessage, SocketUserMessage message)
    {
        await logger.LogMessageAsync($"An error occurred while processing contest submission: {errorMessage}");
        await message.Author.SendMessageAsync(
            "Oops! Something went wrong while processing your submission. Please try again later."
        );
    }

    private static async Task NotifyUserOfDuplicateSubmissionAsync(SocketUserMessage message, IMessage existingMessage)
    {
        await message.DeleteAsync();
        await message.Author.SendMessageAsync(
            $"Bwuh! Y'all already made a contest submission here! {existingMessage.GetJumpUrl()}{Environment.NewLine}Y'all best edit y'alls existing submission or delete it before submitting a new one!!!"
        );
    }
}
