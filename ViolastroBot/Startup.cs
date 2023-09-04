using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.Services;
using ViolastroBot.Services.Jobs;

namespace ViolastroBot;

public sealed class Startup
{
    private DiscordSocketClient _client;
    
    public async Task Initialize()
    {
        IServiceProvider services = ConfigureServices();
        _client = services.GetRequiredService<DiscordSocketClient>();
        _client.Log += Log;
        
        services.GetRequiredService<CommandService>().Log += Log;
        
        await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"));
        await _client.StartAsync();

        await services.GetRequiredService<CommandHandlerService>().InitializeAsync();
        
        JobSchedulerService jobSchedulerService = services.GetRequiredService<JobSchedulerService>();
        await jobSchedulerService.InitializeAsync(_client);
        await jobSchedulerService.ScheduleCronJob<RenameChannelJob>("0 0 * ? * *");
        await jobSchedulerService.ScheduleCronJob<RemoveBirthdayRolesJob>("0 0 10 ? * *");

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

        IServiceCollection services = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlerService>()
            .AddSingleton<MessageHandlerService>()
            .AddSingleton<JobSchedulerService>();

        return services.BuildServiceProvider();
    }
}