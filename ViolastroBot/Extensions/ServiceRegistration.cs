using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Features;
using ViolastroBot.Features.Roulette;

namespace ViolastroBot.Extensions;

public static class ServiceRegistration
{
    public static ServiceProvider ConfigureServices(this IConfiguration configuration)
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
            .Scan(scan =>
                scan.FromAssemblyOf<Startup>()
                    .AddClasses(c => c.AssignableTo<ISingleton>())
                    .AsSelfWithInterfaces()
                    .WithSingletonLifetime()
                    // Explicitly register all RouletteAction subclasses as RouletteAction
                    .AddClasses(c => c.AssignableTo<RouletteAction>())
                    .As<RouletteAction>()
                    .WithSingletonLifetime()
            )
            .BuildServiceProvider();
    }
}
