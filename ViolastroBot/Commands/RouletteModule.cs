using System.Reflection;
using Discord.Commands;
using ViolastroBot.Commands.Preconditions;
using ViolastroBot.Commands.RouletteActions;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Commands;

public sealed class RouletteModule : ModuleBase<SocketCommandContext>
{
    private readonly IServiceProvider _services;
    private readonly Random _random = new();
    private readonly Type[] _rouletteActions = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => t.IsSubclassOf(typeof(RouletteAction)))
        .ToArray();

    public RouletteModule(IServiceProvider services)
    {
        _services = services;
    }
    
    [Command("roulette")]
    [RequireRole(Roles.Moderator)]
    public Task PlayRoulette()
    {
        return _random.Next(0, 100) > 50 ? Task.CompletedTask : ExecuteRandomRouletteAction();
    }

    private Task ExecuteRandomRouletteAction()
    {
        double randomValue = _random.NextDouble() * 100;

        RouletteActionTier selectedTier = randomValue switch
        {
            < 60 => RouletteActionTier.Common,
            < 95 => RouletteActionTier.Uncommon,
            < 99.9 => RouletteActionTier.Rare,
            _ => RouletteActionTier.VeryRare
        };

        Type[] actionsInSelectedTier = _rouletteActions
            .Where(t => t.GetCustomAttribute<RouletteActionTierAttribute>()?.Tier == selectedTier)
            .ToArray();

        Type selectedAction = actionsInSelectedTier[_random.Next(0, actionsInSelectedTier.Length)];

        RouletteAction actionInstance = (RouletteAction)Activator.CreateInstance(selectedAction, _services);
        Console.WriteLine($"Executing {selectedAction.Name}...");

        return actionInstance?.ExecuteAsync(Context);
    }
}