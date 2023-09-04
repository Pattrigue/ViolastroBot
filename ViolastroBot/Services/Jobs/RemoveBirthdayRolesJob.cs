using Discord.WebSocket;
using Quartz;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Services.Jobs;

public sealed class RemoveBirthdayRolesJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        if (context.Scheduler.Context.Get("DiscordClient") is not DiscordSocketClient client)
        {
            return;
        }

        if (client.GetGuild(Guilds.SemagGames) is not { } guild)
        {
            return;
        }

        if (guild.GetRole(Roles.Birthday) is not { } role)
        {
            return;
        }

        await guild.DownloadUsersAsync();

        foreach (SocketGuildUser user in guild.Users)
        {
            if (user.Roles.Contains(role))
            {
                await user.RemoveRoleAsync(role);
            }
        }
    }
}