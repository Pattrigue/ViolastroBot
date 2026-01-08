using Discord;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Logging;

namespace ViolastroBot.Features;

public sealed class WelcomeNewMembers(DiscordSocketClient client, ILoggingService logger) : IStartupTask, ISingleton
{
    public Task InitializeAsync()
    {
        client.UserJoined += UserJoinedAsync;
        return Task.CompletedTask;
    }
    
    private async Task UserJoinedAsync(SocketGuildUser user)
    {
        await logger.LogMessageAsync($"User {user.Mention} joined the server.");

        if (client.GetChannel(Channels.GeneralChannel) is SocketTextChannel channel)
        {
            await channel.SendMessageAsync($"Welcome to the server, {user.Mention}! {new Emoji("👋")}");
        }
    }
}
