using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Services.Logging;

namespace ViolastroBot.Commands;

[Name("Warn")]
public sealed class WarnModule : ModuleBase<SocketCommandContext>
{
    private readonly ILoggingService _logger;
    
    public WarnModule(ILoggingService logger)
    {
        _logger = logger;
    }
        
    [Command("warn")]
    [Discord.Commands.Summary("Warns the mentioned user and gives them the warning role.")]
    [RequireRole(Roles.Moderator)]
    public Task WarnUser([Remainder] string _ = "")
    {
        if (Context.Message.MentionedUsers.Count == 0)
        {
            return Task.CompletedTask;
        }

        SocketGuildUser user = Context.Guild.GetUser(Context.Message.MentionedUsers.First().Id);
        SocketRole warningRole = Context.Guild.GetRole(Roles.Warning);
        
        user.AddRoleAsync(warningRole);
        _logger.LogMessageAsync($"User {user.Mention} has been warned by {Context.User.Mention}.");
        
        return ReplyAsync($"You have been given the warning role for misbehaving {user.Mention}. Please follow the server rules.");
    }
}