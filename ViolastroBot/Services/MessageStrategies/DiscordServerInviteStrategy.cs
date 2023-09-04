using System.Text.RegularExpressions;
using Discord.WebSocket;

namespace ViolastroBot.Services.MessageStrategies;

public partial class DiscordServerInviteStrategy : IMessageStrategy
{
    public async Task ExecuteAsync(SocketUserMessage message)
    {
        if (DiscordInviteRegex().IsMatch(message.Content))
        {
            await message.DeleteAsync();
            await message.Channel.SendMessageAsync("Please don't advertise y'all's Discord servers here!");
        }
    }

    [GeneratedRegex(@"(https:\/\/)?(www\.)?(discord\.gg|discord\.me|discordapp\.com\/invite|discord\.com\/invite)\/([a-z0-9-.]+)?", RegexOptions.IgnoreCase)]
    private static partial Regex DiscordInviteRegex();
}