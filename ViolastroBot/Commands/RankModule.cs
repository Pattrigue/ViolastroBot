using Discord.Commands;
using ViolastroBot.Extensions;
using ViolastroBot.RandomWords;

namespace ViolastroBot.Commands;

public sealed class RankModule : ModuleBase<SocketCommandContext>
{
    [Command("rank")]
    [Summary("Displays the user's rank generated based on their ID as well as the current month.")]
    public Task DisplayRank()
    {
        DateTime date = DateTime.Now;
        int year = date.Year;
        int month = date.Month;

        ulong seed = Context.User.Id + (ulong)month + (ulong)year;

        WordRandomizer wordRandomizer = new(seed);
        
        List<string> words = wordRandomizer.GetRandomWords(1, 3);
        words[0] = words[0].CapitalizeFirstCharacter();
        
        return ReplyAsync($"Rank: {string.Join(' ', words)}.");
    }
}