using Discord.Commands;
using Discord.WebSocket;
using ViolastroBot.Commands.Preconditions;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Commands;

[Name("Minion Toaster")]
public sealed class BirthdayModule : ModuleBase<SocketCommandContext>
{
    [Command("bday")]
    [Discord.Commands.Summary("Assigns the birthday role to the the mentioned user.")]
    [RequireRole(Roles.Moderator)]
    public Task AssignBirthdayRole([Remainder] string _ = "")
    {
        if (Context.Message.MentionedUsers.Count == 0)
        {
            return Task.CompletedTask;
        }

        SocketGuildUser user = Context.Guild.GetUser(Context.Message.MentionedUsers.First().Id);
        SocketRole birthdayRole = Context.Guild.GetRole(Roles.Birthday);

        if (user.Roles.Any(role => role.Id == Roles.Birthday))
        {
            return user.RemoveRoleAsync(birthdayRole);
        }
        
        user.AddRoleAsync(birthdayRole);
        
        return ReplyAsync($"Happy dabby birthday, {user.Mention}! Bwehehe!!");
    }
}