using System.Reflection;
using Discord;
using Discord.Commands;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Features.Roulette;

namespace ViolastroBot.Features.Commands;

[Name("Roulette")]
public sealed class RouletteModule : ModuleBase<SocketCommandContext>
{
    private const int DefaultCooldownInMinutes = 15;
    private const int PremiumCooldownInMinutes = 5;

    private static readonly Dictionary<ulong, DateTimeOffset> Cooldowns = new();

    private readonly RouletteScoreboard _rouletteScoreboard;
    private readonly RouletteAction[] _rouletteActions;
    private readonly Random _random = new();

    private readonly HashSet<string> _scoreParameters = ["score", "scores", "scoreboard", "leaderboard"];

    private readonly List<string> _randomResponses =
    [
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
        "Do not come. Do not come.",
        $"Ya know, speedrunning exists, y'all should try it!{Environment.NewLine}https://speedrun.com/vibrant_venture",
        "<:ViolastroMindBlown:1214884928328568852>",
    ];

    public RouletteModule(RouletteScoreboard rouletteScoreboard, IEnumerable<RouletteAction> rouletteActions)
    {
        _rouletteScoreboard = rouletteScoreboard;
        _randomResponses = _randomResponses.Concat(Jokes.List).ToList();
        _rouletteActions = rouletteActions.ToArray();
    }

    [Command("roulette")]
    [Summary("Plays the roulette.")]
    public async Task PlayRoulette([Remainder] string text = "")
    {
        if (Context.Channel.Id != Channels.BotCommands)
        {
            _ = Task.Run(async () =>
            {
                var reply = await ReplyAsync(
                    $"Ya can only play the roulette in <#{Channels.BotCommands}>, {Context.User.Mention}!!"
                );

                await Context.Message.DeleteAsync();
                await Task.Delay(5000);
                await reply.DeleteAsync();
            });

            return;
        }

        var parameters = text.Split(' ');

        if (parameters.Length >= 1 && _scoreParameters.Contains(parameters[0].ToLower()))
        {
            await DisplayRouletteScore();
            return;
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

    private async Task DisplayRouletteScore()
    {
        if (Context.Message.MentionedUsers.Count == 1)
        {
            await _rouletteScoreboard.DisplayScoreAsync(
                Context.Guild,
                Context.Channel,
                Context.Message.MentionedUsers.First()
            );
            return;
        }

        await _rouletteScoreboard.DisplayScoreboardAsync(Context.Guild, Context.Channel);
    }

    private async Task<bool> IsUserOnCooldown()
    {
        if (!Cooldowns.TryGetValue(Context.User.Id, out var lastUsed))
        {
            return false;
        }

        var cooldownInMinutes = DefaultCooldownInMinutes;

        if (Context.Guild.GetUser(Context.User.Id).PremiumSince != null)
        {
            cooldownInMinutes = PremiumCooldownInMinutes;
        }

        var cooldown = TimeSpan.FromMinutes(cooldownInMinutes);
        var difference = DateTimeOffset.Now - lastUsed;

        if (cooldown <= difference)
        {
            return false;
        }

        var waitTime = cooldown - difference;

        var minutes = waitTime.Minutes;
        var seconds = waitTime.Seconds;

        await ReplyAsync(
            $"Ya gotta wait {minutes} minutes and {seconds} seconds before ya can play the roulette again, {Context.User.Mention}!!"
        );
        await Context.Message.AddReactionAsync(new Emoji("🤓"));

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
        var randomValue = _random.NextDouble() * 100;

        var selectedTier = randomValue switch
        {
            < 60 => RouletteActionTier.Common,
            < 95 => RouletteActionTier.Uncommon,
            < 99.9 => RouletteActionTier.Rare,
            _ => RouletteActionTier.VeryRare,
        };

        var actionsInSelectedTier = _rouletteActions
            .Where(a => a.GetType().GetCustomAttribute<RouletteActionTierAttribute>()?.Tier == selectedTier)
            .ToArray();

        if (actionsInSelectedTier.Length == 0)
        {
            throw new InvalidOperationException($"No roulette actions found in tier {selectedTier}!");
        }

        var selectedAction = actionsInSelectedTier[_random.Next(0, actionsInSelectedTier.Length)];

        await selectedAction.ExecuteAsync(Context);
    }

    private Task ReplyWithRandomResponse()
    {
        var response = _randomResponses[new Random().Next(0, _randomResponses.Count)];

        return ReplyAsync(response);
    }
}
