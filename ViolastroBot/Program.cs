using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Services;
using ViolastroBot.Services.Logging;
using ViolastroBot.Services.MessageStrategies;

namespace ViolastroBot;

public static class Program
{
    public static async Task Main()
    {
        var configuration = BuildConfiguration();
        var services = ConfigureServices(configuration);

        var startup = new Startup(services);
        await startup.Initialize();
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static ServiceProvider ConfigureServices(IConfiguration configuration)
    {
        DiscordSocketConfig config = new()
        {
            LogLevel = LogSeverity.Info,
            GatewayIntents =
                GatewayIntents.Guilds
                | GatewayIntents.GuildMessages
                | GatewayIntents.GuildMessageReactions
                | GatewayIntents.GuildMembers
                | GatewayIntents.GuildBans
                | GatewayIntents.GuildEmojis
                | GatewayIntents.DirectMessages
                | GatewayIntents.DirectMessageReactions
                | GatewayIntents.MessageContent,
        };

        return new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton(configuration)
            .Configure<ContestChannelSettings>(configuration.GetSection("ContestChannelSettings"))
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlerService>()
            .AddSingleton<MessageHandlerService>()
            .AddSingleton<WelcomeMessageService>()
            .AddSingleton<JobSchedulerService>()
            .AddSingleton<SubscriberRoleService>()
            .AddSingleton<ScoreboardService>()
            .AddSingleton<ILoggingService, DiscordLoggingService>()
            .AddSingleton<IMessageStrategy, ViolasstroReactionStrategy>()
            .AddSingleton<IMessageStrategy, DuplicateMessageStrategy>()
            .AddSingleton<IMessageStrategy, FeedbackSuggestionsReactionStrategy>()
            .AddSingleton<IMessageStrategy, DiscordServerInviteStrategy>()
            .AddSingleton<IMessageStrategy, QuestionsAnswersStrategy>()
            .AddSingleton<IMessageStrategy, OffensiveWordChecker>()
            .AddSingleton<IMessageStrategy, ContestSubmissionStrategy>()
            .BuildServiceProvider();
    }
}