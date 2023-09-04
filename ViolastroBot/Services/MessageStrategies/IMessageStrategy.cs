using Discord.WebSocket;

namespace ViolastroBot.Services.MessageStrategies;

public interface IMessageStrategy
{
    Task ExecuteAsync(SocketUserMessage message);
}