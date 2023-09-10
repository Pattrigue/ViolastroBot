﻿using System.Reflection;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.Commands.Roulette;
using ViolastroBot.Commands.Roulette.Actions;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Services;

namespace ViolastroBot.Commands;

[Name("Roulette")]
public sealed class RouletteModule : ModuleBase<SocketCommandContext>
{
    private const int DefaultCooldownInMinutes = 15;
    private const int PremiumCooldownInMinutes = 5;
    
    private static readonly Dictionary<ulong, DateTimeOffset> Cooldowns = new();

    private readonly ScoreboardService _scoreboardService;
    private readonly Random _random = new();
    private readonly Dictionary<Type, RouletteAction> _rouletteActions;
    
    private readonly HashSet<string> _scoreParameters = new()
    {
        "score",
        "scores",
        "scoreboard",
        "leaderboard"
    };
    
    private readonly List<string> _randomResponses = new()
    {
        "Deleting the server in 5 minutes...",
        "I'm feeling full of beans!",
        "We think it is good, but we design things on certaim things.",
        "public static void main string args",
        "I HATE USING GREEN TO THE SPIKES",
        "I Can Fix you.",
        "it requires... RNG?????????????????????????????????",
        "Would U need sign a deal???.",
        "( yep, DAILY! ]",
        "https://tenor.com/view/boing-bounce-thingamabob-the-thingamabob-thing-gif-24819899",
        "https://media.discordapp.net/attachments/1110626262784938025/1118087579754049586/image0-43.gif",
        "https://media.discordapp.net/attachments/884162046772531200/902911462752808990/caption.gif",
        "https://media.discordapp.net/attachments/709674340504829974/906236927571820704/image0-141.gif",
        "<:keoiki:1026916538911035402>",
        "Do not come. Do not come."
    };
    
    public RouletteModule(IServiceProvider services)
    {
        _scoreboardService = services.GetRequiredService<ScoreboardService>();
        _randomResponses = _randomResponses.Concat(Jokes.List).ToList();
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
    public async Task PlayRoulette([Remainder] string text = "")
    {
        if (_scoreParameters.Contains(text.ToLowerInvariant()))
        {
            await _scoreboardService.DisplayScoreboardAsync(Context.Guild, Context.Channel);
            return;
        }
        
        foreach (RouletteAction action in _rouletteActions.Values)
        {
            if (action is AssignNewRole assignNewRole)
            {
                await assignNewRole.ExecuteAsync(Context);
                return;
            }
        }
        
        if (await IsUserOnCooldown())
        {
            return;
        }

        Cooldowns[Context.User.Id] = DateTimeOffset.Now;

        if (await UserLostRoulette())
        {
            return;
        }

        await ExecuteRandomRouletteAction();
    }

    private async Task<bool> IsUserOnCooldown()
    {
        if (!Cooldowns.TryGetValue(Context.User.Id, out DateTimeOffset lastUsed))
        {
            return false;
        }
        
        int cooldownInMinutes = DefaultCooldownInMinutes;

        if (Context.Guild.GetUser(Context.User.Id).PremiumSince != null)
        {
            cooldownInMinutes = PremiumCooldownInMinutes;
        }

        TimeSpan cooldown = TimeSpan.FromMinutes(cooldownInMinutes);
        TimeSpan difference = DateTimeOffset.Now - lastUsed;

        if (cooldown <= difference)
        {
            return false;
        }
        
        TimeSpan waitTime = cooldown - difference;

        int minutes = waitTime.Minutes;
        int seconds = waitTime.Seconds;

        await ReplyAsync($"Ya gotta wait {minutes} minutes and {seconds} seconds before ya can play the roulette again, {Context.User.Mention}!!");
        
        return true;
    }
    
    private async Task<bool> UserLostRoulette()
    {
        if (_random.Next(0, 100) > 50)
        {
            if (_random.Next(0, 100) < 75)
            {
                await ReplyWithRandomResponse();
            }
            else
            {
                await Context.Message.AddReactionAsync(new Emoji("🇱"));

                if (_random.Next(0, 100) < 20)
                {
                    await ReplyAsync("Womp womp!");
                }
            }

            return true;
        }

        return false;
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

        RouletteAction[] actionsInSelectedTier = _rouletteActions
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

    private Task ReplyWithRandomResponse()
    {
        string response = _randomResponses[new Random().Next(0, _randomResponses.Count)];

        return ReplyAsync(response);
    }
}