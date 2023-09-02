using Discord.Commands;
using ViolastroBot.RandomWords;

namespace ViolastroBot.Commands;

public class RankModule : ModuleBase<SocketCommandContext>
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
        words[0] = CapitalizeFirstLetter(words[0]);
        
        return ReplyAsync($"Rank: {string.Join(' ', words)}.");
    }
    
    private static string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        char[] chars = input.ToCharArray();
        chars[0] = char.ToUpper(chars[0]);
        
        return new string(chars);
    }
}