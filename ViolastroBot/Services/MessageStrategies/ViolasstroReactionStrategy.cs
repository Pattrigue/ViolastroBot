using Discord;
using Discord.WebSocket;

namespace ViolastroBot.Services.MessageStrategies;

public class ViolasstroReactionStrategy : IMessageStrategy
{
    public async Task ExecuteAsync(SocketUserMessage message)
    {
        if (message.Content.Contains("<:violasstro:741764537312608256>", StringComparison.OrdinalIgnoreCase))
        {
            await message.AddReactionAsync(new Emoji("😳"));
        }
    }
}