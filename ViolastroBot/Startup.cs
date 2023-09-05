using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.Services;
using ViolastroBot.Services.Jobs;
using ViolastroBot.Services.Logging;
using ViolastroBot.Services.MessageStrategies;

namespace ViolastroBot;

public sealed class Startup
{
    private readonly IServiceProvider _services;
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commandService;
    private readonly JobSchedulerService _jobSchedulerService;

    public Startup()
    {
        _services = ConfigureServices();
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
        string botToken = Environment.GetEnvironmentVariable("VIOLASTRO_BOT_TOKEN");
        await _client.LoginAsync(TokenType.Bot, botToken);
        await _client.SetActivityAsync(new Game("Vibrant Venture"));
        await _client.StartAsync();
    }

    private async Task InitializeServices()
    {
        IEnumerable<Type> serviceTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(ServiceBase).IsAssignableFrom(p) && !p.IsAbstract);

        foreach (Type type in serviceTypes)
        {
            ServiceBase service = (ServiceBase)_services.GetService(type);
            
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
            .AddSingleton<WelcomeMessageService>()
            .AddSingleton<JobSchedulerService>()
            .AddSingleton<ILoggingService, DiscordLoggingService>()
            .AddSingleton<IMessageStrategy, ViolasstroReactionStrategy>()
            .AddSingleton<IMessageStrategy, DuplicateMessageStrategy>()
            .AddSingleton<IMessageStrategy, FeedbackSuggestionsReactionStrategy>()
            .AddSingleton<IMessageStrategy, DiscordServerInviteStrategy>()
            .AddSingleton<IMessageStrategy, QuestionsAnswersStrategy>()
            .AddSingleton<IMessageStrategy, OffensiveWordChecker>()
            .BuildServiceProvider();
    }
}