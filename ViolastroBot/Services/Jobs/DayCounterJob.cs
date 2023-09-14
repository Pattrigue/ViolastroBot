using Discord.WebSocket;
using Quartz;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Extensions;
using ViolastroBot.RandomWords;

namespace ViolastroBot.Services.Jobs;

public sealed class DayCounterJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        if (context.Scheduler.Context.Get("DiscordClient") is not DiscordSocketClient client)
        {
            return;
        }

        if (client.GetChannel(Channels.GeneralChannel) is not SocketTextChannel channel)
        {
            return;
        }

        WordRandomizer wordRandomizer = new();

        int year = DateTime.Now.Year;
        int dayInYear = DateTime.Now.DayOfYear;

        string word = wordRandomizer.GetRandomWord();
        string message = $"Day {dayInYear} of {year}. Today is {word} day!";

        await channel.SendMessageAsync(message);
    }
}