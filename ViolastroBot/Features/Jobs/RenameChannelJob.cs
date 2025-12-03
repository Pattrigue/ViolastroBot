using Discord.WebSocket;
using Quartz;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Extensions;
using ViolastroBot.Features.RandomWords;

namespace ViolastroBot.Features.Jobs;

public sealed class RenameChannelJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        if (context.Scheduler.Context.Get("DiscordClient") is not DiscordSocketClient client)
        {
            return;
        }

        if (client.GetChannel(Channels.RandomChannel) is not SocketTextChannel channel)
        {
            return;
        }

        WordRandomizer wordRandomizer = new();

        var randomWords = wordRandomizer.GetRandomWords(1, 3);
        var newName = string.Join(" ", randomWords);

        await channel.ModifyAsync(x => x.Name = newName);
        await channel.SendMessageAsync($"{newName.CapitalizeFirstCharacter()}!");
    }
}
