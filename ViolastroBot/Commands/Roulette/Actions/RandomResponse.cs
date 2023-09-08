using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Commands.Roulette.Actions;

/// <summary>
/// Gets a random response from a list of responses and replies with it.
/// </summary>
[RouletteActionTier(RouletteActionTier.Common)]
public sealed class RandomResponse : RouletteAction
{
    private readonly List<string> _responses = new()
    {
        "Deleting the server in 5 minutes...",
        "I'm feeling full of beans!",
        "We think it is good, but we design things on certaim things.",
        "public static void main string args",
        "I HATE USING GREEN TO THE SPIKES",
        "I Can Fix you.",
        "it requires... RNG?????????????????????????????????",
        "Would U need sign a deal???.",
        "( yep, DAILY! ]"
    };
    
    public RandomResponse(IServiceProvider services) : base(services)
    {
        _responses = _responses.Concat(Jokes.List).ToList();
    }

    protected override Task ExecuteAsync()
    {
        string response = _responses[new Random().Next(0, _responses.Count)];

        return ReplyAsync(response);
    }
}