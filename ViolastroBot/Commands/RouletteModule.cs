using System.Reflection;
using Discord;
using Discord.Commands;
using ViolastroBot.Commands.Preconditions;
using ViolastroBot.Commands.Roulette;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Commands;

public sealed class RouletteModule : ModuleBase<SocketCommandContext>
{
    private const int RouletteCooldownInHours = 1;
    
    private static readonly Dictionary<ulong, DateTimeOffset> Cooldowns = new();
    
    private readonly IServiceProvider _services;
    private readonly Random _random = new();
    private readonly Type[] _rouletteActions;

    public RouletteModule(IServiceProvider services)
    {
        _services = services;
        _rouletteActions = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(RouletteAction)))
            .ToArray();
    }
    
    [Command("roulette")]
    [RequireRole(Roles.Moderator)]
    public async Task PlayRoulette()
    {
        if (Cooldowns.TryGetValue(Context.User.Id, out DateTimeOffset lastUsed))
        {
            TimeSpan cooldown = TimeSpan.FromHours(RouletteCooldownInHours);
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
            // react with a regional indicator L emoji
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

        Type[] actionsInSelectedTier = _rouletteActions
            .Where(t => t.GetCustomAttribute<RouletteActionTierAttribute>()?.Tier == selectedTier)
            .ToArray();

        Type selectedAction = actionsInSelectedTier[_random.Next(0, actionsInSelectedTier.Length)];
        RouletteAction actionInstance = (RouletteAction)Activator.CreateInstance(selectedAction, _services);

        Console.WriteLine($"Executing {selectedAction.Name}...");

        await actionInstance?.ExecuteAsync(Context);
    }
}