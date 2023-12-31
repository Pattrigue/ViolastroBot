﻿using Discord.WebSocket;
using Quartz;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Extensions;
using ViolastroBot.RandomWords;

namespace ViolastroBot.Services.Jobs;

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

        List<string> randomWords = wordRandomizer.GetRandomWords(1, 3);
        string newName = string.Join(" ", randomWords);

        await channel.ModifyAsync(x => x.Name = newName);
        await channel.SendMessageAsync($"{newName.CapitalizeFirstCharacter()}!");
    }
}