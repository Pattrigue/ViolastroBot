using Discord.WebSocket;

namespace ViolastroBot.Features.MessageStrategies;

public interface IMessageStrategy : ISingleton
{
    public bool ShouldCancelOthers() => false;

    Task<bool> ExecuteAsync(SocketUserMessage message);
}
