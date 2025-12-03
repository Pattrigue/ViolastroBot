using Discord.Commands;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Logging;

namespace ViolastroBot.Features.Commands;

[Name("Warn")]
public sealed class WarnModule(ILoggingService logger) : ModuleBase<SocketCommandContext>
{
    [Command("warn")]
    [Summary("Warns the mentioned user and gives them the warning role.")]
    [RequireRole(Roles.Moderator)]
    public Task WarnUser([Remainder] string _ = "")
    {
        if (Context.Message.MentionedUsers.Count == 0)
        {
            return Task.CompletedTask;
        }

        var user = Context.Guild.GetUser(Context.Message.MentionedUsers.First().Id);
        var warningRole = Context.Guild.GetRole(Roles.Warning);

        user.AddRoleAsync(warningRole);
        logger.LogMessageAsync($"User {user.Mention} has been warned by {Context.User.Mention}.");

        return ReplyAsync(
            $"You have been given the warning role for misbehaving {user.Mention}. Please follow the server rules."
        );
    }
}
