using System.Reflection;
using Discord;
using Discord.Commands;
using ViolastroBot.Commands.Roulette;

namespace ViolastroBot.Commands;

[Name("Roulette")]
public sealed class RouletteModule : ModuleBase<SocketCommandContext>
{
    private const int DefaultCooldownInMinutes = 15;
    private const int PremiumCooldownInMinutes = 5;
    
    private static readonly Dictionary<ulong, DateTimeOffset> Cooldowns = new();

    private readonly Random _random = new();
    private readonly Dictionary<Type, RouletteAction> _rouletteActions;
    
    public RouletteModule(IServiceProvider services)
    {
        _rouletteActions = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(RouletteAction)))
            .ToDictionary(
                t => t,
                t => (RouletteAction)Activator.CreateInstance(t, services)
            );
            
    }
    
    [Command("roulette")]
    [Summary("Plays the roulette.")]
    public async Task PlayRoulette()
    {
        if (Cooldowns.TryGetValue(Context.User.Id, out DateTimeOffset lastUsed))
        {
            int cooldownInMinutes = DefaultCooldownInMinutes;
            
            if (Context.Guild.GetUser(Context.User.Id).PremiumSince != null)
            {
                cooldownInMinutes = PremiumCooldownInMinutes;
            }
            
            TimeSpan cooldown = TimeSpan.FromMinutes(cooldownInMinutes);
            TimeSpan difference = DateTimeOffset.Now - lastUsed;

            if (cooldown > difference)
            {
                TimeSpan waitTime = cooldown - difference;
                
                int minutes = waitTime.Minutes;
                int seconds = waitTime.Seconds;

                await ReplyAsync($"Ya gotta wait {minutes} minutes and {seconds} seconds before ya can play the roulette again!!");
                return;
            }
        }

        Cooldowns[Context.User.Id] = DateTimeOffset.Now;

        if (_random.Next(0, 100) > 50)
        {
            await Context.Message.AddReactionAsync(new Emoji("🇱"));
            return;
        }

        await ExecuteRandomRouletteAction();
    }

    private async Task ExecuteRandomRouletteAction()
    {
        double randomValue = _random.NextDouble() * 100;

        RouletteActionTier selectedTier = randomValue switch
        {
            < 60 => RouletteActionTier.Common,
            < 95 => RouletteActionTier.Uncommon,
            < 99.9 => RouletteActionTier.Rare,
            _ => RouletteActionTier.VeryRare
        };

        var actionsInSelectedTier = _rouletteActions
            .Where(kvp => kvp.Key.GetCustomAttribute<RouletteActionTierAttribute>()?.Tier == selectedTier)
            .Select(kvp => kvp.Value)
            .ToArray();

        if (actionsInSelectedTier.Length == 0)
        {
            Console.WriteLine($"No roulette actions found in tier {selectedTier}!");
            return;
        }

        RouletteAction selectedAction = actionsInSelectedTier[_random.Next(0, actionsInSelectedTier.Length)];
        
        Console.WriteLine($"Executing {selectedAction.GetType().Name}...");

        await selectedAction.ExecuteAsync(Context);
    }
}