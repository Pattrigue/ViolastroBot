using System.Globalization;
using Discord.Commands;
using Discord.WebSocket;

namespace ViolastroBot.Commands;

[Name("Joined")]
public sealed class JoinedModule : ModuleBase<SocketCommandContext>
{
    [Command("joined")]
    [Summary("Displays the date and time the user (or the mentioned user) joined the server.")]
    public Task DisplayJoinedDate([Remainder] string _ = "")
    {
        // If another user is mentioned in the message, use that user's join date instead of the author's.
        SocketGuildUser user = Context.Message.MentionedUsers.Count > 0
            ? Context.Guild.GetUser(Context.Message.MentionedUsers.First().Id)
            : Context.Guild.GetUser(Context.User.Id);
        
        string username = user.GlobalName ?? user.Username;
        string joined = user.JoinedAt?.ToString("MMMM dd, yyyy h:mm tt", CultureInfo.InvariantCulture) ?? "unknown";
        
        return ReplyAsync($"{username} joined this server on {joined}.");
    }
}