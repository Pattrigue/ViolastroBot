﻿using Discord.Commands;
using ViolastroBot.Extensions;
using ViolastroBot.RandomWords;

namespace ViolastroBot.Commands;

[Name("Rank")]
public sealed class RankModule : ModuleBase<SocketCommandContext>
{
    [Command("rank")]
    [Summary("Displays the user's rank for the month.")]
    public Task DisplayRank([Remainder] string text = null)
    {
        DateTime date = DateTime.Now;
        int year = date.Year;
        int month = date.Month;

        ulong id;
        string rankText;
        
        if (!string.IsNullOrEmpty(text) && text.ToLowerInvariant() == "bot")
        {
            id = Context.Client.CurrentUser.Id;
            rankText = "My rank";
        }
        else
        {
            id = Context.User.Id;
            rankText = "Rank";
        }

        ulong seed = id + (ulong)month * (ulong)year;

        WordRandomizer wordRandomizer = new(seed);
        
        List<string> words = wordRandomizer.GetRandomWords(1, 3);
        words[0] = words[0].CapitalizeFirstCharacter();
        
        return ReplyAsync($"{rankText}: {string.Join(' ', words)}.");
    }
}