using Discord.Commands;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Services.Logging;

namespace ViolastroBot.Commands;

[Name("Unwarn")]
public sealed class UnwarnModule(ILoggingService logger) : ModuleBase<SocketCommandContext>
{
    [Command("unwarn")]
    [Summary("Removes the warning role from the mentioned user.")]
    [RequireRole(Roles.Moderator)]
    public Task UnwarnUser([Remainder] string _ = "")
    {
        if (Context.Message.MentionedUsers.Count == 0)
        {
            return Task.CompletedTask;
        }

        var user = Context.Guild.GetUser(Context.Message.MentionedUsers.First().Id);
        var warningRole = Context.Guild.GetRole(Roles.Warning);

        if (user.Roles.All(role => role.Id != Roles.Warning))
        {
            return Task.CompletedTask;
        }

        logger.LogMessageAsync($"User {user.Mention} has been unwarned by {Context.User.Mention}.");

        return user.RemoveRoleAsync(warningRole);
    }
}
