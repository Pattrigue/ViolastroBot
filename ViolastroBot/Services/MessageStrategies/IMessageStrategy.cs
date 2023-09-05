using Discord.WebSocket;

namespace ViolastroBot.Services.MessageStrategies;

public interface IMessageStrategy
{
    public bool ShouldCancelOthers() => false;

    Task<bool> ExecuteAsync(SocketUserMessage message);
}