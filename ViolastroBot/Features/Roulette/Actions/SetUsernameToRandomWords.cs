using ViolastroBot.Extensions;
using ViolastroBot.Features.RandomWords;

namespace ViolastroBot.Features.Roulette.Actions;

/// <summary>
/// Sets the user's nickname to a random name.
/// </summary>
[RouletteActionTier(RouletteActionTier.Uncommon)]
public sealed class SetUsernameToRandomWords(IServiceProvider services) : RouletteAction(services)
{
    private const int MaxAttempts = 25;

    protected override async Task ExecuteAsync()
    {
        var wordRandomizer = new WordRandomizer();
        var name = string.Empty;

        for (var i = 0; i < MaxAttempts; i++)
        {
            var words = wordRandomizer.GetRandomWords(1, 3);
            name = string.Join(" ", words).CapitalizeFirstCharacterInEachWord();

            var currentName = Context.User.GlobalName ?? Context.User.Username;
            var nickname = $"{name} ({currentName})";

            if (nickname.Length <= 32)
            {
                await Context.Guild.GetUser(Context.User.Id).ModifyAsync(properties => properties.Nickname = nickname);
                await ReplyAsync($"Bwehehe!! Ya name is now \"{name}\"!!");
                return;
            }
        }

        await ReplyAsync($"I tried to name y'all \"{name}\", but Discord ain't allowing it!!");
    }
}
