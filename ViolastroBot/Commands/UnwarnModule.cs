using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Services.Logging;

namespace ViolastroBot.Commands;

public sealed class UnwarnModule : ModuleBase<SocketCommandContext>
{
    private readonly ILoggingService _logger;
    
    public UnwarnModule(ILoggingService logger)
    {
        _logger = logger;
    }
    
    [Command("unwarn")]
    [Discord.Commands.Summary("Removes the warning role from the mentioned user.")]
    [RequireRole(Roles.Moderator)]
    public Task UnwarnUser([Remainder] string _ = "")
    {
        if (Context.Message.MentionedUsers.Count == 0)
        {
            return Task.CompletedTask;
        }

        SocketGuildUser user = Context.Guild.GetUser(Context.Message.MentionedUsers.First().Id);
        SocketRole warningRole = Context.Guild.GetRole(Roles.Warning);

        if (user.Roles.All(role => role.Id != Roles.Warning))
        {
            return Task.CompletedTask;
        }
        
        _logger.LogMessageAsync($"User {user.Mention} has been unwarned by {Context.User.Mention}.");
        
        return user.RemoveRoleAsync(warningRole);
    }
}