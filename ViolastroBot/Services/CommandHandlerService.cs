using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Services;

public sealed class CommandHandlerService : ServiceBase
{
    private const char CommandPrefix = '!';
    
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _services;

    public CommandHandlerService(IServiceProvider services)
    {
        _commands = services.GetRequiredService<CommandService>();
        _client = services.GetRequiredService<DiscordSocketClient>();
        _services = services;

        _client.MessageReceived += OnMessageReceivedAsync;
    } 
    
    public override async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    private async Task OnMessageReceivedAsync(SocketMessage messageParam)
    {
        // Don't process the command if it was a system message
        if (messageParam is not SocketUserMessage message || message.Author.IsBot)
        {
            return;
        }
        
        // Create a number to track where the prefix ends and the command begins
        var argPos = 0;

        // Determine if the message is a command based on the prefix 
        if (!message.HasCharPrefix(CommandPrefix, ref argPos))
        {
            return;
        }

        if (message.HasMentionPrefix(_client.CurrentUser, ref argPos))
        {
            return;
        }
        
        if (message.Channel.Id == Channels.ContestSubmissions)
        {
            return;
        }

        // Create a WebSocket-based command context based on the message
        var context = new SocketCommandContext(_client, message);

        // Execute the command with the command context we just
        // created, along with the service provider for precondition checks.
        await _commands.ExecuteAsync(context: context, argPos: argPos, services: _services);
    }
}