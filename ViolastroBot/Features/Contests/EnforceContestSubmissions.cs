using Discord;
using Discord.WebSocket;
using ViolastroBot.Features.Commands;

namespace ViolastroBot.Features.Contests;

public sealed class EnforceContestSubmissions : ISingleton
{
    private readonly DiscordSocketClient _client;
    private readonly ContestSubmission _submission;

    public EnforceContestSubmissions(DiscordSocketClient client, ContestSubmission submission)
    {
        _client = client;
        _submission = submission;

        _client.MessageReceived += OnMessageReceivedAsync;
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

        if (!_submission.IsContestChannel(message.Channel.Id))
        {
            return;
        }

        // allow only !submit (optionally allow !submit with args, but that's your command anyway)
        if (!message.Content.StartsWith(ContestSubmitModule.CommandName, StringComparison.OrdinalIgnoreCase))
        {
            await message.DeleteAsync();
            await message.Author.SendMessageAsync(
                "Bwagh! Y'all can only use the `!submit` command in the contest submissions channel!"
            );
        }
    }
}
