using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Services.Logging;

namespace ViolastroBot.Services.MessageStrategies;

public sealed class ContestSubmissionStrategy : IMessageStrategy
{
    private static bool _isProcessingSubmission;
    
    private readonly ILoggingService _logger;
    
    public ContestSubmissionStrategy(IServiceProvider services) 
    {
        _logger = services.GetRequiredService<ILoggingService>();
    }
    
    public async Task<bool> ExecuteAsync(SocketUserMessage message)
    {
        if (message.Channel.Id != Channels.ContestSubmissions)
        {
            return false;
        }

        if (message.Channel is not SocketTextChannel channel)
        {
            return false;
        }
        
        if (!message.Content.StartsWith("!submit", StringComparison.OrdinalIgnoreCase))
        {
            await message.DeleteAsync();
            await message.Author.SendMessageAsync("Bwagh! Y'all need to use the `!submit` command in the contest submissions channel to submit your message!");
            
            return false;
        }
        
        if (_isProcessingSubmission)
        {
            await message.Author.SendMessageAsync("AAAAAHHHHH!!! Ya know, I'm already processing a submission! Gimme a sec to finish up before y'all submit another one!");
            
            return false;
        }
        
        ulong lastMessageId = message.Id;

        try
        {
            _isProcessingSubmission = true;
                        
            await message.Author.SendMessageAsync("Appreciate y'all for submitting a contest entry! I'm checkin' to see if y'all already submitted a message...");
            
            // Loop to fetch and process messages until all messages are checked
            while (true)
            {
                IEnumerable<IMessage> messages = await channel.GetMessagesAsync(lastMessageId, Direction.Before).FlattenAsync();
                List<IMessage> messageList = messages.ToList();

                if (!messageList.Any()) break; // Exit if no more messages to process

                // Check if the user has already submitted a message
                IMessage existingMessage = messageList.FirstOrDefault(msg => msg.Author.Id == message.Author.Id);

                if (existingMessage != null)
                {
                    await message.DeleteAsync();
                    await message.Author.SendMessageAsync($"Bwuh! Y'all already made a contest submission here! {existingMessage.GetJumpUrl()}{Environment.NewLine}Y'all best edit y'alls existing submission or delete it before submitting a new one!!!");

                    return false;
                }

                await Task.Delay(1000);
                lastMessageId = messageList.Last().Id; // Update last message ID for the next batch
            }
            
            await message.AddReactionAsync(new Emoji("👍"));

            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogMessageAsync($"An error occurred while processing contest submission: {ex.Message}");
            await message.Author.SendMessageAsync("Oops! Something went wrong while processing your submission. Please try again later.");
            
            return false;
        }
        finally
        {
            _isProcessingSubmission = false;
        }
    }
}