﻿using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Commands;

[Name("Rule")]
public sealed partial class RuleModule : ModuleBase<SocketCommandContext>
{
    [Command("rule")]
    [Summary("Displays the rule with the specified number in chat.")]
    public Task SendRule(string ruleNumberText)
    {
        SocketTextChannel rulesChannel = Context.Guild.GetTextChannel(Channels.Rules);
        IMessage rulesMessage = rulesChannel.GetMessagesAsync(1).FlattenAsync().Result.First();
        
        string[] rulesMessageLines = rulesMessage.Content.Split('\n');
        
        foreach (string line in rulesMessageLines)
        {
            if (Regex().IsMatch(line) && line.StartsWith(ruleNumberText))
            {
                return ReplyAsync(line);
            }
        }

        return ReplyAsync("That rule don't exist, ya fool! Bwehehe!!");
    }

    [GeneratedRegex("^[0-9].+$")]
    private static partial Regex Regex();
}