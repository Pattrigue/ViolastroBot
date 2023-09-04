using Discord;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Services;

public sealed class WelcomeMessageService : ServiceBase
{
    private readonly DiscordSocketClient _client;

    public WelcomeMessageService(DiscordSocketClient client)
    {
        _client = client;
        _client.UserJoined += UserJoinedAsync;
    }

    private async Task UserJoinedAsync(SocketGuildUser user)
    {
        if (_client.GetChannel(Channels.GeneralChannel) is SocketTextChannel channel)
        {
            await channel.SendMessageAsync($"Welcome to the server, {user.Mention}! {new Emoji("👋")}");
        }
    }
}