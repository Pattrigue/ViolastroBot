using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Commands;

[Name("Rule")]
public sealed partial class RuleModule : ModuleBase<SocketCommandContext>
{
    [Command("rule")]
    [Summary("Displays the rule with the specified number in chat.")]
    public Task SendRule(string ruleNumberText)
    {
        var rulesChannel = Context.Guild.GetTextChannel(Channels.Rules);
        var rulesMessage = rulesChannel.GetMessagesAsync(1).FlattenAsync().Result.First();

        var rulesMessageLines = rulesMessage.Content.Split('\n');

        foreach (var line in rulesMessageLines)
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
