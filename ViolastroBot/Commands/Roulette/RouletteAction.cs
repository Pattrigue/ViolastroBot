using Discord.Commands;

namespace ViolastroBot.Commands.Roulette;

public abstract class RouletteAction
{
    protected SocketCommandContext Context { get; private set; }

    protected readonly IServiceProvider Services;

    protected RouletteAction(IServiceProvider services)
    {
        Services = services;
    }

    public async Task ExecuteAsync(SocketCommandContext context)
    {
        Context = context;
        await ExecuteAsync();
    }

    protected abstract Task ExecuteAsync();

    protected Task ReplyAsync(string message) => Context.Channel.SendMessageAsync(message);
}
