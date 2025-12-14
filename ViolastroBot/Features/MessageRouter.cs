using Discord.WebSocket;
using ViolastroBot.Features.MessageStrategies;

namespace ViolastroBot.Features;

public sealed class MessageRouter : ISingleton, IActivateOnStartup
{
    private readonly DiscordSocketClient _client;
    private readonly List<IMessageStrategy> _strategies;

    public MessageRouter(DiscordSocketClient client, IEnumerable<IMessageStrategy> strategies)
    {
        _client = client;
        _strategies = strategies.OrderByDescending(strategy => strategy.ShouldCancelOthers()).ToList();
        _client.MessageReceived += HandleMessageReceivedAsync;
    }

    private async Task HandleMessageReceivedAsync(SocketMessage message)
    {
        if (message is not SocketUserMessage userMessage || message.Author.IsBot)
        {
            return;
        }

        foreach (var strategy in _strategies)
        {
            var success = await strategy.ExecuteAsync(userMessage);

            if (success && strategy.ShouldCancelOthers())
            {
                break;
            }
        }
    }
}
