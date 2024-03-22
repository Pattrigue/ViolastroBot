using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Services.Logging;

namespace ViolastroBot.Services.MessageStrategies;

public sealed class ContestSubmissionStrategy : IMessageStrategy
{
    private const string Prefix = "!submit";
    
    private static bool _isProcessingSubmission;

    private readonly ILoggingService _logger;

    public ContestSubmissionStrategy(IServiceProvider services)
    {
        _logger = services.GetRequiredService<ILoggingService>();
    }

    public async Task<bool> ExecuteAsync(SocketUserMessage message)
    {
        if (!IsMessageValidForSubmission(message))
        {
            return false;
        }

        return await ProcessSubmissionAsync(message);
    }


    private async Task<bool> ProcessSubmissionAsync(SocketUserMessage message)
    {
        if (_isProcessingSubmission)
        {
            await message.Author.SendMessageAsync("AAAAAHHHHH!!! Ya know, I'm already processing a submission! Gimme a sec to finish up before y'all submit another one!");
            return false;
        }

        try
        {
            _isProcessingSubmission = true;
            await message.Author.SendMessageAsync("Appreciate y'all for submitting a contest entry! I'm checkin' to see if y'all already submitted a message...");

            return await CheckForExistingSubmissionAsync(message);
        }
        catch (Exception ex)
        {
            await LogErrorAndNotifyUserAsync(ex.Message, message);
            return false;
        }
        finally
        {
            _isProcessingSubmission = false;
        }
    }

    private async Task<bool> CheckForExistingSubmissionAsync(SocketUserMessage message)
    {
        ulong lastMessageId = message.Id;
        
        if (message.Channel is not SocketTextChannel channel)
        {
            return false;
        }

        while (true)
        {
            IEnumerable<IMessage> messages = await channel.GetMessagesAsync(lastMessageId, Direction.Before).FlattenAsync();
            List<IMessage> messageList = messages.ToList();

            if (!messageList.Any()) break;

            IMessage existingMessage = messageList.FirstOrDefault(msg => msg.Author.Id == message.Author.Id);
            
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
        await _logger.LogMessageAsync($"An error occurred while processing contest submission: {errorMessage}");
        await message.Author.SendMessageAsync("Oops! Something went wrong while processing your submission. Please try again later.");
    }

    private static bool IsMessageValidForSubmission(SocketUserMessage message)
    {
        if (message.Channel.Id != Channels.ContestSubmissions || message.Channel is not SocketTextChannel)
        {
            return false;
        }

        if (!message.Content.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            NotifyUserInvalidCommand(message);
            return false;
        }

        return true;
    }

    private static async void NotifyUserInvalidCommand(SocketUserMessage message)
    {
        await message.DeleteAsync();
        await message.Author.SendMessageAsync("Bwagh! Y'all need to use the `!submit` command in the contest submissions channel to submit your message!");
    }
    
    private static async Task NotifyUserOfDuplicateSubmissionAsync(SocketUserMessage message, IMessage existingMessage)
    {
        await message.DeleteAsync();
        await message.Author.SendMessageAsync($"Bwuh! Y'all already made a contest submission here! {existingMessage.GetJumpUrl()}{Environment.NewLine}Y'all best edit y'alls existing submission or delete it before submitting a new one!!!");
    }
}