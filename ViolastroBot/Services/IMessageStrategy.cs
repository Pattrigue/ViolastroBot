using Discord.WebSocket;

namespace ViolastroBot.Services;

public interface IMessageStrategy
{
    Task ExecuteAsync(SocketUserMessage message);
}