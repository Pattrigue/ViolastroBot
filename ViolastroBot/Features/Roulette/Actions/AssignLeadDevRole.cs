using Discord.Commands;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Features.Roulette.Actions;

/// <summary>
/// Temporarily assigns the user the Lead Developer role, then removes it again.
/// </summary>
[RouletteActionTier(RouletteActionTier.VeryRare)]
public sealed class AssignLeadDevRole(DiscordSocketClient client, RouletteScoreboard rouletteScoreboard)
    : RouletteAction,
        IStartupTask
{
    private const int RoleDurationInMinutes = 30;

    public async Task InitializeAsync()
    {
        Console.WriteLine("Checking for users with the Lead Developer role that are not the server owner...");

        foreach (var guild in client.Guilds)
        {
            await guild.DownloadUsersAsync();

            foreach (var user in guild.Users)
            {
                if (user.Roles.Any(role => role.Id == Roles.LeadDeveloper) && user.Id != guild.OwnerId)
                {
                    Console.WriteLine(
                        $"Removing Lead Developer role from {user.Mention} because they are not the server owner."
                    );
                    await user.RemoveRoleAsync(guild.GetRole(Roles.LeadDeveloper));
                }
            }
        }
    }

    protected override async Task ExecuteAsync()
    {
        var user = Context.Guild.GetUser(Context.User.Id);
        var leadDevRole = Context.Guild.GetRole(Roles.LeadDeveloper);

        await rouletteScoreboard.IncrementScoreboardAsync(Context.Guild, Context.User, 5);

        if (user.Roles.Any(r => r.Id == leadDevRole.Id))
        {
            await ReplyAsync(
                $"Uhm!! Ya see, I was gonna give ya the {leadDevRole.Mention} role, but ya already have it!! Bwehehe!"
            );
            return;
        }

        await user.AddRoleAsync(leadDevRole);
        await ReplyAsync($"You are now the Lead Developer for {RoleDurationInMinutes} minutes! Bwaaahaha!!");

        _ = Task.Delay(TimeSpan.FromMinutes(RoleDurationInMinutes))
            .ContinueWith(async _ => await user.RemoveRoleAsync(leadDevRole));
    }
}
