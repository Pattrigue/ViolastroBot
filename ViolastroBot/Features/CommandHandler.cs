using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.Features.Contests;

namespace ViolastroBot.Features;

public sealed class CommandHandler : IStartupTask, ISingleton
{
    public const char CommandPrefix = '!';

    private readonly CommandService _commands;
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _services;
    private readonly ContestSubmissions _contestSubmissions;

    public CommandHandler(IServiceProvider services, ContestSubmissions contestSubmissions)
    {
        _commands = services.GetRequiredService<CommandService>();
        _client = services.GetRequiredService<DiscordSocketClient>();
        _services = services;
        _contestSubmissions = contestSubmissions;

        _client.MessageReceived += OnMessageReceivedAsync;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    private async Task OnMessageReceivedAsync(SocketMessage messageParam)
    {
        if (messageParam is not SocketUserMessage message || message.Author.IsBot)
        {
            return;
        }

        var isContestChannel = _contestSubmissions.IsContestChannel(message.Channel.Id);

        if (isContestChannel)
        {
            await _contestSubmissions.HandleAsync(message);
            return;
        }

        var argPos = 0;

        if (!message.HasCharPrefix(CommandPrefix, ref argPos))
        {
            return;
        }

        var context = new SocketCommandContext(_client, message);
        await _commands.ExecuteAsync(context: context, argPos: argPos, services: _services);
    }
}
