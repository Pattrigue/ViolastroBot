using Discord.Commands;
using ViolastroBot.Extensions;
using ViolastroBot.Features.RandomWords;

namespace ViolastroBot.Features.Commands;

[Name("Rank")]
public sealed class RankModule : ModuleBase<SocketCommandContext>
{
    [Command("rank")]
    [Summary("Displays the user's rank for the month.")]
    public Task DisplayRank([Remainder] string text = null)
    {
        var (year, month, _) = DateTime.Now;

        ulong id;
        string rankText;

        if (!string.IsNullOrEmpty(text) && text.Equals("bot", StringComparison.InvariantCultureIgnoreCase))
        {
            id = Context.Client.CurrentUser.Id;
            rankText = "My rank";
        }
        else
        {
            id = Context.User.Id;
            rankText = "Rank";
        }

        var seed = id + (ulong)month * (ulong)year;

        WordRandomizer wordRandomizer = new(seed);

        var words = wordRandomizer.GetRandomWords(1, 3);
        words[0] = words[0].CapitalizeFirstCharacter();

        return ReplyAsync($"{rankText}: {string.Join(' ', words)}.");
    }
}
