using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace ViolastroBot.Commands;

public sealed class UnwarnModule : ModuleBase<SocketCommandContext>
{
    [Command("unwarn")]
    [Discord.Commands.Summary("Removes the warning role from the mentioned user.")]
    [RequireRole(Roles.Moderator)]
    public Task UnwarnUser([Remainder] string _ = "")
    {
        if (Context.Message.MentionedUsers.Count == 0) return Task.CompletedTask;

        SocketGuildUser user = Context.Guild.GetUser(Context.Message.MentionedUsers.First().Id);
        SocketRole warningRole = Context.Guild.GetRole(Roles.Warning);
        
        if (user.Roles.All(role => role.Id != Roles.Warning)) return Task.CompletedTask;
        
        return user.RemoveRoleAsync(warningRole);
    }
}