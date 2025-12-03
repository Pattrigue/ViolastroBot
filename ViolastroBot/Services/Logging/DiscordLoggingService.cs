using Discord;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Services.Logging;

public sealed class DiscordLoggingService : ServiceBase, ILoggingService
{
    private readonly DiscordSocketClient _client;

    public DiscordLoggingService(DiscordSocketClient client)
    {
        _client = client;
        _client.Ready += OnReady;
    }

    public Task LogMessageAsync(string message)
    {
        if (_client.GetChannel(Channels.LogChannel) is IMessageChannel logChannel)
        {
            return logChannel.SendMessageAsync(message);
        }

        return Task.CompletedTask;
    }

    private async Task OnReady()
    {
        await LogMessageAsync("ViolastroBot.NET is ready and running!");
    }
}
