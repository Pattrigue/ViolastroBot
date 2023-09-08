using System.Reflection;
using Discord.Commands;
using ViolastroBot.Commands.Preconditions;
using ViolastroBot.Commands.RouletteActions;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Commands;

public sealed class RouletteModule : ModuleBase<SocketCommandContext>
{
    private readonly IServiceProvider _services;
    private readonly Type[] _rouletteActions;

    private static readonly string[] Responses = 
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

    public RouletteModule(IServiceProvider services)
    {
        _services = services;
        Responses.Append<>(Jokes.List.ToArray());
        _rouletteActions = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(RouletteAction)))
            .ToArray();
    }
    
    [Command("roulette")]
    [RequireRole(Roles.Moderator)]
    public Task PlayRoulette()
    {
        Random random = new Random();

        if (random.Next(0, 100) > 50)
        {
            return random.Next(0, 100) < 75 ? SelectRandomResponse() : Task.CompletedTask;
        }

        return ExecuteRandomRouletteAction(random);
    }

    private Task ExecuteRandomRouletteAction(Random random)
    {
        Type action = _rouletteActions[random.Next(_rouletteActions.Length)];
        RouletteAction actionInstance = (RouletteAction)Activator.CreateInstance(action, _services);

        Console.WriteLine($"Executing {action.Name}...");

        return actionInstance?.ExecuteAsync(Context);
    }

    private async Task SelectRandomResponse()
    {
        string response = Responses[new Random().Next(0, Responses.Length)];

        await ReplyAsync(response);
    }
}