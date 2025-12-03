using Discord.Commands;

namespace ViolastroBot.Features.Roulette;

public abstract class RouletteAction(IServiceProvider services)
{
    protected SocketCommandContext Context { get; private set; }

    protected readonly IServiceProvider Services = services;

    public async Task ExecuteAsync(SocketCommandContext context)
    {
        Context = context;
        await ExecuteAsync();
    }

    protected abstract Task ExecuteAsync();

    protected Task ReplyAsync(string message) => Context.Channel.SendMessageAsync(message);
}
