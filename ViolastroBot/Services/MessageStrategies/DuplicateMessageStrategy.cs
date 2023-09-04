using Discord;
using Discord.WebSocket;
using ViolastroBot.Services.MessageStrategies;

public sealed class DuplicateMessageStrategy : IMessageStrategy
{
    private const int Limit = 4;  // Fetch 4 previous messages to compare with the current message

    public async Task ExecuteAsync(SocketUserMessage message)
    {
        if (message.Channel is SocketTextChannel channel)
        {
            // Fetch the last 4 messages (excluding the current message)
            List<IMessage> messages = (await channel.GetMessagesAsync(message, Direction.Before, Limit)
                .FlattenAsync())
                .ToList();

            // Include the current message for comparison
            messages.Add(message);

            // Check if we have 5 messages in total
            if (messages.Count == 5)
            {
                string currentMessageContent = message.Content;
                bool areAllMessagesSame = messages.All(m => m.Content == currentMessageContent);

                if (areAllMessagesSame)
                {
                    await channel.SendMessageAsync("Y'all best stop spammin'!");
                }
            }
        }
    }
}
