using Discord;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Services.Logging;

namespace ViolastroBot.Services;

public sealed class WelcomeMessageService : ServiceBase
{
    private readonly DiscordSocketClient _client;
    private readonly ILoggingService _logger;

    public WelcomeMessageService(DiscordSocketClient client, ILoggingService logger)
    {
        _client = client;
        _logger = logger;
        _client.UserJoined += UserJoinedAsync;
    }

    private async Task UserJoinedAsync(SocketGuildUser user)
    {
        await _logger.LogMessageAsync($"User {user.Mention} joined the server.");
        
        if (_client.GetChannel(Channels.GeneralChannel) is SocketTextChannel channel)
        {
            await channel.SendMessageAsync($"Welcome to the server, {user.Mention}! {new Emoji("👋")}");
        }
    }
}