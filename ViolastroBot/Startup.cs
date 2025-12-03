using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.Services;
using ViolastroBot.Services.Jobs;

namespace ViolastroBot;

public sealed class Startup
{
    private readonly IServiceProvider _services;
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commandService;
    private readonly JobSchedulerService _jobSchedulerService;

    public Startup(IServiceProvider services)
    {
        _services = services;
        _client = _services.GetRequiredService<DiscordSocketClient>();
        _commandService = _services.GetRequiredService<CommandService>();
        _jobSchedulerService = _services.GetRequiredService<JobSchedulerService>();
    }

    public async Task Initialize()
    {
        RegisterLogging();
        await LoginAndStartBot();
        await InitializeServices();
        await ScheduleJobs();
        await WaitForCompletion();
    }

    private void RegisterLogging()
    {
        _client.Log += Log;
        _commandService.Log += Log;
    }

    private async Task LoginAndStartBot()
    {
        var botToken = Environment.GetEnvironmentVariable("VIOLASTRO_BOT_TOKEN");
        await _client.LoginAsync(TokenType.Bot, botToken);
        await _client.SetActivityAsync(new Game("Vibrant Venture"));
        await _client.StartAsync();
    }

    private async Task InitializeServices()
    {
        var serviceTypes = AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(ServiceBase).IsAssignableFrom(p) && !p.IsAbstract);

        foreach (var type in serviceTypes)
        {
            var service = (ServiceBase?)_services.GetService(type);

            if (service != null)
            {
                await service.InitializeAsync();
            }
        }
    }

    private async Task ScheduleJobs()
    {
        await _jobSchedulerService.ScheduleCronJob<RenameChannelJob>("0 0 * ? * *");
        await _jobSchedulerService.ScheduleCronJob<RemoveBirthdayRolesJob>("0 0 10 ? * *");
        await _jobSchedulerService.ScheduleCronJob<DayCounterJob>("0 0 17 ? * *");
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private static Task WaitForCompletion() => Task.Delay(-1);
}