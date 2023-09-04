using System.Text.RegularExpressions;
using Discord.WebSocket;

namespace ViolastroBot.Services.MessageStrategies;

public sealed partial class DiscordServerInviteStrategy : IMessageStrategy
{
    private static readonly HashSet<string> AllowedInvites = new(StringComparer.OrdinalIgnoreCase) { "SSUTPCU" };

    public async Task ExecuteAsync(SocketUserMessage message)
    {
        Match match = DiscordInviteRegex().Match(message.Content);

        if (!match.Success)
        {
            return;
        }

        // Extract the invite code using the named group
        string inviteCode = match.Groups["InviteCode"].Value;

        // Check if the invite code is in the HashSet of allowed invites
        if (AllowedInvites.Contains(inviteCode))
        {
            return;
        }

        // Not an allowed invite, so delete and warn
        await message.DeleteAsync();
        await message.Channel.SendMessageAsync("Please don't advertise y'all's Discord servers here!");
    }

    [GeneratedRegex(@"(https:\/\/)?(www\.)?(discord\.gg|discord\.me|discordapp\.com\/invite|discord\.com\/invite)\/(?<InviteCode>[a-z0-9-.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex DiscordInviteRegex();
}