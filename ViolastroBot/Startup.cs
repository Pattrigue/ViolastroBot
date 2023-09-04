using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.Services;
using ViolastroBot.Services.Jobs;
using ViolastroBot.Services.MessageStrategies;

namespace ViolastroBot;

public sealed class Startup
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commandService;
    private readonly CommandHandlerService _commandHandlerService;
    private readonly MessageHandlerService _messageHandlerService;
    private readonly JobSchedulerService _jobSchedulerService;

    public Startup()
    {
        IServiceProvider services = ConfigureServices();
        _client = services.GetRequiredService<DiscordSocketClient>();
        _commandService = services.GetRequiredService<CommandService>();
        _commandHandlerService = services.GetRequiredService<CommandHandlerService>();
        _messageHandlerService = services.GetRequiredService<MessageHandlerService>();
        _jobSchedulerService = services.GetRequiredService<JobSchedulerService>();
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
        string botToken = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");
        await _client.LoginAsync(TokenType.Bot, botToken);
        await _client.StartAsync();
    }

    private async Task InitializeServices()
    {
        await _commandHandlerService.InitializeAsync();
        await _messageHandlerService.InitializeAsync();
        await _jobSchedulerService.InitializeAsync(_client);
    }

    private async Task ScheduleJobs()
    {
        await _jobSchedulerService.ScheduleCronJob<RenameChannelJob>("0 0 * ? * *");
        await _jobSchedulerService.ScheduleCronJob<RemoveBirthdayRolesJob>("0 0 10 ? * *");
    }

    private async Task WaitForCompletion()
    {
        await Task.Delay(-1);
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private static IServiceProvider ConfigureServices()
    {
        DiscordSocketConfig config = new()
        {
            LogLevel = LogSeverity.Info,
            GatewayIntents = GatewayIntents.Guilds
                             | GatewayIntents.GuildMessages
                             | GatewayIntents.GuildMessageReactions
                             | GatewayIntents.GuildMembers
                             | GatewayIntents.GuildBans
                             | GatewayIntents.GuildEmojis
                             | GatewayIntents.DirectMessages
                             | GatewayIntents.DirectMessageReactions
                             | GatewayIntents.MessageContent
        };

        return new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlerService>()
            .AddSingleton<MessageHandlerService>()
            .AddSingleton<IMessageStrategy, ViolasstroReactionStrategy>()
            .AddSingleton<JobSchedulerService>()
            .BuildServiceProvider();
    }
}