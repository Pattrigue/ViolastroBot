using Discord.Commands;

namespace ViolastroBot.Features.Roulette;

public abstract class RouletteAction : ISingleton
{
    public abstract Task ExecuteAsync(SocketCommandContext context);

    protected static Task ReplyAsync(SocketCommandContext context, string message) =>
        context.Channel.SendMessageAsync(message);
}
