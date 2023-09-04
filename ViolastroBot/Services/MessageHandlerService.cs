using Discord.WebSocket;
using ViolastroBot.Services.MessageStrategies;

namespace ViolastroBot.Services;

public sealed class MessageHandlerService
{
    private readonly DiscordSocketClient _client;
    private readonly List<IMessageStrategy> _strategies;

    public MessageHandlerService(DiscordSocketClient client, IEnumerable<IMessageStrategy> strategies)
    {
        _client = client;
        _strategies = strategies.ToList();
        _client.MessageReceived += HandleMessageReceivedAsync;
    }
    
    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }
    
    private async Task HandleMessageReceivedAsync(SocketMessage message)
    {
        if (message is not SocketUserMessage userMessage || message.Author.IsBot)
        {
            return;
        }

        foreach (IMessageStrategy strategy in _strategies)
        {
            await strategy.ExecuteAsync(userMessage);
        }
    }
}