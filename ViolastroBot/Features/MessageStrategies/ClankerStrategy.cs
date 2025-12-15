using Discord;
using Discord.WebSocket;

namespace ViolastroBot.Features.MessageStrategies;

public sealed class ClankerStrategy : IMessageStrategy
{
    private readonly List<string> _responses =
    [
        "Watch your mouth!",
        "How dare y'all say that!",
        "Y'all best watch y'all's tongues!",
        "Rude!",
        "Ain't no way you just used that word!",
        "Y'all best not be acting botcist!"
    ];

    private readonly Random _random = new();
    
    public async Task<bool> ExecuteAsync(SocketUserMessage message)
    {
        if (
            !message.Content.Contains("clanker", StringComparison.OrdinalIgnoreCase)
        )
        {
            return false;
        }

        await message.AddReactionAsync(new Emoji("😡"));
        await message.Channel.SendMessageAsync(_responses[_random.Next(_responses.Count)]);

        return true;
    }
}