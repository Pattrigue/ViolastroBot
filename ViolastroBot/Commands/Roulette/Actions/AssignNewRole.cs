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
public sealed class AssignNewRole(IServiceProvider services) : RouletteAction(services)
{
    private readonly ScoreboardService _scoreboardService = services.GetRequiredService<ScoreboardService>();

    protected override async Task ExecuteAsync()
    {
        var role = Context.Guild.GetRole(Roles.NewRole);

        var usersWithRole = Context
            .Guild.Users.Where(user => user.Roles.Any(userRole => userRole.Id == Roles.NewRole))
            .ToList();

        var userToReceiveRole = Context.Guild.GetUser(Context.User.Id);

        var reply = new StringBuilder();

        await _scoreboardService.IncrementScoreboardAsync(Context.Guild, Context.User);

        if (usersWithRole.Count > 0)
        {
            // Check if the current user already has the role
            if (usersWithRole.Any(u => u.Id == userToReceiveRole.Id))
            {
                await ReplyAsync(
                    $"Erm!! This is awkward... Ya see, I was gonna give ya the {role.Mention} role, but ya already have it! Bwehehe!!"
                );
                return;
            }

            await RemoveRoleFromCurrentUsersWithRole(usersWithRole, role, reply);
        }

        // Add role to the current user
        await userToReceiveRole.AddRoleAsync(role);
        reply.Insert(0, $"Bwehehe!! {userToReceiveRole.Mention} now has the {role.Mention} role!{Environment.NewLine}");

        await Context.Channel.SendMessageAsync(reply.ToString(), allowedMentions: AllowedMentions.None);
    }

    private static async Task RemoveRoleFromCurrentUsersWithRole(
        List<SocketGuildUser> usersWithRole,
        IRole role,
        StringBuilder reply
    )
    {
        var mentions = new List<string>();

        foreach (var user in usersWithRole)
        {
            mentions.Add(user.Mention);
            await user.RemoveRoleAsync(role);
        }

        string mentionString;
        string haveOrHas;

        switch (mentions.Count)
        {
            case 1:
                mentionString = mentions[0];
                haveOrHas = "has";
                break;
            case 2:
                mentionString = $"{mentions[0]} and {mentions[1]}";
                haveOrHas = "have";
                break;
            default:
                mentionString = string.Join(", ", mentions.Take(mentions.Count - 1)) + ", and " + mentions.Last();
                haveOrHas = "have";
                break;
        }

        reply.AppendLine($"That means {mentionString} no longer {haveOrHas} it - too bad!");
    }
}
