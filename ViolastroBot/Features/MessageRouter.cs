using Discord.WebSocket;
using ViolastroBot.Features.Chat;
using ViolastroBot.Features.MessageStrategies;

namespace ViolastroBot.Features;

public sealed class MessageRouter : ISingleton, IActivateOnStartup
{
    private readonly DiscordSocketClient _client;
    private readonly List<IMessageStrategy> _strategies;
    private readonly ViolastroChat _chat;

    public MessageRouter(DiscordSocketClient client, IEnumerable<IMessageStrategy> strategies, ViolastroChat chat)
    {
        _client = client;
        _chat = chat;
        _strategies = strategies.OrderByDescending(strategy => strategy.ShouldCancelOthers()).ToList();
        _client.MessageReceived += HandleMessageReceivedAsync;
    }

    private async Task HandleMessageReceivedAsync(SocketMessage message)
    {
        if (message is not SocketUserMessage userMessage || message.Author.IsBot)
        {
            return;
        }

        var handled = false;

        foreach (var strategy in _strategies)
        {
            var success = await strategy.ExecuteAsync(userMessage);
            handled |= success;

            if (success && strategy.ShouldCancelOthers())
            {
                break;
            }
        }

        if (!handled)
        {
            _chat.TryRespond(userMessage);
        }
    }
}
