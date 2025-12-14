using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.Features;
using ViolastroBot.Features.Jobs;

namespace ViolastroBot;

public sealed class Startup
{
    private readonly IServiceProvider _services;
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commandService;
    private readonly JobScheduler _jobScheduler;

    public Startup(IServiceProvider services)
    {
        _services = services;
        _client = _services.GetRequiredService<DiscordSocketClient>();
        _commandService = _services.GetRequiredService<CommandService>();
        _jobScheduler = _services.GetRequiredService<JobScheduler>();
    }

    public async Task Run()
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
        _ = _services.GetServices<IActivateOnStartup>().ToArray();

        // Run async initialization for the subset that needs it
        foreach (var task in _services.GetServices<IStartupTask>())
        {
            await task.InitializeAsync();
        }
    }

    private async Task ScheduleJobs()
    {
        await _jobScheduler.ScheduleCronJob<RenameChannelJob>("0 0 * ? * *");
        await _jobScheduler.ScheduleCronJob<RemoveBirthdayRolesJob>("0 0 10 ? * *");
        await _jobScheduler.ScheduleCronJob<DayCounterJob>("0 0 17 ? * *");
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private static Task WaitForCompletion() => Task.Delay(-1);
}
