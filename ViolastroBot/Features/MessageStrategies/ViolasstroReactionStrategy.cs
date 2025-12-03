using Discord;
using Discord.WebSocket;

namespace ViolastroBot.Features.MessageStrategies;

public sealed class ViolasstroReactionStrategy : IMessageStrategy, ISingleton
{
    public async Task<bool> ExecuteAsync(SocketUserMessage message)
    {
        if (
            !message.Content.Contains("<:violasstro:741764537312608256>", StringComparison.OrdinalIgnoreCase)
            && !message.Content.Contains("<:violasstro2:1055115670628601887>", StringComparison.OrdinalIgnoreCase)
        )
        {
            return false;
        }

        await message.AddReactionAsync(new Emoji("😳"));

        return true;
    }
}
