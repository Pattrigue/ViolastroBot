using System.Text;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Services;

namespace ViolastroBot.Commands.Roulette.Actions;

/// <summary>
/// Assigns a new role to the user, and removes it from the user who currently has it.
/// </summary>
[RouletteActionTier(RouletteActionTier.Uncommon)]
public sealed class AssignNewRole : RouletteAction
{
    private readonly ScoreboardService _scoreboardService;
    
    public AssignNewRole(IServiceProvider services) : base(services)
    {
        _scoreboardService = services.GetRequiredService<ScoreboardService>();
    }

    protected override async Task ExecuteAsync()
    {
        SocketRole role = Context.Guild.GetRole(Roles.NewRole);

        SocketGuildUser userWithRole = Context.Guild.Users.FirstOrDefault(user =>
            user.Roles.Any(userRole => userRole.Id == Roles.NewRole));

        SocketGuildUser userToReceiveRole = Context.Guild.GetUser(Context.User.Id);

        StringBuilder reply = new StringBuilder();

        if (userWithRole != null)
        {
            if (userWithRole.Id == userToReceiveRole.Id)
            {
                await ReplyAsync($"Erm!! This is awkward... Ya see, I was gonna give ya the {role.Mention} role, but ya already have it! Bwehehe!!");
                return;
            }

            reply.AppendLine($"That means {userWithRole.Mention} no longer has it - too bad!");
            await userWithRole.RemoveRoleAsync(Roles.NewRole);
        }

        await userToReceiveRole.AddRoleAsync(Roles.NewRole);
        reply.Insert(0, $"Bwehehe!! {userToReceiveRole.Mention} now has the {role.Mention} role! ");

        await _scoreboardService.IncrementScoreboardAsync(Context.Guild, Context.User);
        await ReplyAsync(reply.ToString());
    }
}