using Discord.WebSocket;
using Quartz;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Features.RandomWords;

namespace ViolastroBot.Features.Jobs;

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

        var year = DateTime.Now.Year;
        var dayInYear = DateTime.Now.DayOfYear;

        var word = wordRandomizer.GetRandomWord();
        var message = $"Day {dayInYear} of {year}. Today is {word} day!";

        await channel.SendMessageAsync(message);
    }
}
