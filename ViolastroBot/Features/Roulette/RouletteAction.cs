using Discord.Commands;

namespace ViolastroBot.Features.Roulette;

public abstract class RouletteAction
{
    private SocketCommandContext? _context;

    protected SocketCommandContext Context =>
        _context ?? throw new InvalidOperationException("Context not set. Did you call ExecuteAsync(context)?");

    public async Task ExecuteAsync(SocketCommandContext context)
    {
        _context = context;

        try
        {
            await ExecuteAsync();
        }
        finally
        {
            _context = null; // avoid accidental reuse
        }
    }

    protected abstract Task ExecuteAsync();

    protected Task ReplyAsync(string message) => Context.Channel.SendMessageAsync(message);
}
