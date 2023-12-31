﻿using System.Text.RegularExpressions;
using Discord.WebSocket;
using ViolastroBot.Services.Logging;

namespace ViolastroBot.Services.MessageStrategies;

public sealed partial class DiscordServerInviteStrategy : IMessageStrategy
{
    private static readonly HashSet<string> AllowedInvites = new(StringComparer.OrdinalIgnoreCase) { "SSUTPCU" };
    
    private readonly ILoggingService _logger;
    
    public DiscordServerInviteStrategy(ILoggingService logger)
    {
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(SocketUserMessage message)
    {
        Match match = DiscordInviteRegex().Match(message.Content);

        if (!match.Success)
        {
            return false;
        }

        // Extract the invite code using the named group
        string inviteCode = match.Groups["InviteCode"].Value;

        // Check if the invite code is in the HashSet of allowed invites
        if (AllowedInvites.Contains(inviteCode))
        {
            return false;
        }

        // Not an allowed invite, so delete and warn
        await message.DeleteAsync();
        await message.Channel.SendMessageAsync("Please don't advertise y'all's Discord servers here!");
        await _logger.LogMessageAsync($"User {message.Author.Mention} tried to advertise a Discord server.{Environment.NewLine}Please make sure they don't spam!");
        
        return true;
    }

    [GeneratedRegex(@"(https:\/\/)?(www\.)?(discord\.gg|discord\.me|discordapp\.com\/invite|discord\.com\/invite)\/(?<InviteCode>[a-z0-9-.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex DiscordInviteRegex();
}