using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Commands.Roulette.Actions;

/// <summary>
/// Temporarily assigns the user the Lead Developer role, then removes it again.
/// </summary>
[RouletteActionTier(RouletteActionTier.VeryRare)]
public sealed class AssignLeadDevRole : RouletteAction
{
    private const int RoleDurationInMinutes = 30;
    
    private readonly DiscordSocketClient _client;
    
    public AssignLeadDevRole(IServiceProvider services) : base(services)
    {
        _client = services.GetRequiredService<DiscordSocketClient>();
        _client.Ready += OnReady;
    }

    ~AssignLeadDevRole()
    {
        _client.Ready -= OnReady;
    }

    protected override async Task ExecuteAsync()
    {
        SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
        SocketRole leadDevRole = Context.Guild.GetRole(Roles.LeadDeveloper);

        if (user.Roles.Any(r => r.Id == leadDevRole.Id))
        {
            return;
        }

        await user.AddRoleAsync(leadDevRole);
        await ReplyAsync($"You are now the Lead Developer for {RoleDurationInMinutes} minutes! Bwaaahaha!!");

        _ = Task.Delay(TimeSpan.FromMinutes(RoleDurationInMinutes))
            .ContinueWith(async _ => await user.RemoveRoleAsync(leadDevRole));
    }

    private async Task OnReady()
    {
        foreach (SocketGuild guild in _client.Guilds)
        {
            await guild.DownloadUsersAsync();
            
            foreach (SocketGuildUser user in guild.Users)
            {
                if (user.Roles.Any(role => role.Id == Roles.LeadDeveloper) && user.Id != guild.OwnerId)
                {
                    await user.RemoveRoleAsync(guild.GetRole(Roles.LeadDeveloper));
                }
            }
        }
    }
}