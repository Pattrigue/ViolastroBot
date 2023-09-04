using Discord;
using Discord.WebSocket;

namespace ViolastroBot.Services.MessageStrategies;

public sealed class ViolasstroReactionStrategy : IMessageStrategy
{
    public async Task ExecuteAsync(SocketUserMessage message)
    {
        if (message.Content.Contains("<:violasstro:741764537312608256>", StringComparison.OrdinalIgnoreCase)
            || message.Content.Contains("<:violasstro2:1055115670628601887>", StringComparison.OrdinalIgnoreCase))
        {
            await message.AddReactionAsync(new Emoji("😳"));
        }
    }
}