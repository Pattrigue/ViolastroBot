using Discord.WebSocket;

namespace ViolastroBot.Features.MessageStrategies;

public interface IMessageStrategy
{
    public bool ShouldCancelOthers() => false;

    Task<bool> ExecuteAsync(SocketUserMessage message);
}
