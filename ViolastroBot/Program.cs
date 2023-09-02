using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ViolastroBot.Commands;

namespace ViolastroBot;

public sealed class Program
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly CommandHandler _commandHandler;
        
    private Program()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            // How much logging do you want to see?
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
                
            // If you or another service needs to do anything with messages
            // (eg. checking Reactions, checking the content of edited/deleted messages),
            // you must set the MessageCacheSize. You may adjust the number as needed.
            //MessageCacheSize = 50,
        });
            
        _commands = new CommandService(new CommandServiceConfig
        {
            // Again, log level:
            LogLevel = LogSeverity.Info,
                
            // There's a few more properties you can set,
            // for example, case-insensitive commands.
            CaseSensitiveCommands = false,
            IgnoreExtraArgs = false
        });
        
        _commandHandler = new CommandHandler(_client, _commands);
        
        // Subscribe the logging handler to both the client and the CommandService.
        _client.Log += Log;
        _commands.Log += Log;
    }
    
    public static Task Main(string[] args) => new Program().MainAsync();

    private async Task MainAsync()
    {
        await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"));
        await _client.StartAsync();
        
        await _commandHandler.InstallCommandsAsync();
        
        // Block this task until the program is closed.
        await Task.Delay(-1);
    }
    
    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}