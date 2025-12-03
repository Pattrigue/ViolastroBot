using Discord;
using Discord.WebSocket;
using ViolastroBot.Features.Commands;

namespace ViolastroBot.Features.Contests;

public sealed class EnforceContestSubmissions(DiscordSocketClient client, ContestSubmissions contestSubmissions)
    : ISingleton, IStartupTask
{
    public Task InitializeAsync()
    {
        client.MessageReceived += OnMessageReceivedAsync;
        return Task.CompletedTask;
    }

    private async Task OnMessageReceivedAsync(SocketMessage rawMessage)
    {
        if (rawMessage is not SocketUserMessage message)
        {
            return;
        }

        if (message.Author.IsBot)
        {
            return;
        }

        if (!contestSubmissions.IsContestChannel(message.Channel.Id))
        {
            return;
        }

        if (!message.Content.StartsWith(ContestSubmitModule.CommandName, StringComparison.OrdinalIgnoreCase))
        {
            await message.DeleteAsync();
            await message.Author.SendMessageAsync(
                "Bwagh! Y'all can only use the `!submit` command in the contest submissions channel!"
            );
        }
    }
}
