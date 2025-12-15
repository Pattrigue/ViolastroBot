using Discord.Commands;
using ViolastroBot.Extensions;
using ViolastroBot.Features.RandomWords;

namespace ViolastroBot.Features.Roulette.Actions;

/// <summary>
/// Sets the user's nickname to a random name.
/// </summary>
[RouletteActionTier(RouletteActionTier.Uncommon)]
public sealed class SetUsernameToRandomWords : RouletteAction
{
    private const int MaxAttempts = 25;

    public override async Task ExecuteAsync(SocketCommandContext context)
    {
        var wordRandomizer = new WordRandomizer();
        var name = string.Empty;

        for (var i = 0; i < MaxAttempts; i++)
        {
            var words = wordRandomizer.GetRandomWords(1, 3);
            name = string.Join(" ", words).CapitalizeFirstCharacterInEachWord();

            var currentName = context.User.GlobalName ?? context.User.Username;
            var nickname = $"{name} ({currentName})";

            if (nickname.Length <= 32)
            {
                await context.Guild.GetUser(context.User.Id).ModifyAsync(properties => properties.Nickname = nickname);
                await ReplyAsync(context, $"Bwehehe!! Ya name is now \"{name}\"!!");
                return;
            }
        }

        await ReplyAsync(context, $"I tried to name y'all \"{name}\", but Discord ain't allowing it!!");
    }
}
