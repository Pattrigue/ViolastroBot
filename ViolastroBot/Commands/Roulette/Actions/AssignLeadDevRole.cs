using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Services;

namespace ViolastroBot.Commands.Roulette.Actions;

/// <summary>
/// Temporarily assigns the user the Lead Developer role, then removes it again.
/// </summary>
[RouletteActionTier(RouletteActionTier.VeryRare)]
public sealed class AssignLeadDevRole : RouletteAction
{
    private const int RoleDurationInMinutes = 30;
    
    private readonly DiscordSocketClient _client;
    private readonly ScoreboardService _scoreboardService;
    
    public AssignLeadDevRole(IServiceProvider services) : base(services)
    {
        _client = services.GetRequiredService<DiscordSocketClient>();
        _client.Ready += OnReady;
        _scoreboardService = services.GetRequiredService<ScoreboardService>();
    }

    ~AssignLeadDevRole()
    {
        _client.Ready -= OnReady;
    }

    protected override async Task ExecuteAsync()
    {
        var user = Context.Guild.GetUser(Context.User.Id);
        var leadDevRole = Context.Guild.GetRole(Roles.LeadDeveloper);

        await _scoreboardService.IncrementScoreboardAsync(Context.Guild, Context.User, 5);
        
        if (user.Roles.Any(r => r.Id == leadDevRole.Id))
        {
            await ReplyAsync($"Uhm!! Ya see, I was gonna give ya the {leadDevRole.Mention} role, but ya already have it!! Bwehehe!");
            return;
        }

        await user.AddRoleAsync(leadDevRole);
        await ReplyAsync($"You are now the Lead Developer for {RoleDurationInMinutes} minutes! Bwaaahaha!!");

        _ = Task.Delay(TimeSpan.FromMinutes(RoleDurationInMinutes))
            .ContinueWith(async _ => await user.RemoveRoleAsync(leadDevRole));
    }

    private async Task OnReady()
    {
        Console.WriteLine("Checking for users with the Lead Developer role that are not the server owner...");
        
        foreach (var guild in _client.Guilds)
        {
            await guild.DownloadUsersAsync();
            
            foreach (var user in guild.Users)
            {
                if (user.Roles.Any(role => role.Id == Roles.LeadDeveloper) && user.Id != guild.OwnerId)
                {
                    Console.WriteLine($"Removing Lead Developer role from {user.Mention} because they are not the server owner.");
                    await user.RemoveRoleAsync(guild.GetRole(Roles.LeadDeveloper));
                }
            }
        }
    }
}