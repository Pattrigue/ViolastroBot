using Discord;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Services.MessageStrategies;

public class FeedbackSuggestionsReactionStrategy : IMessageStrategy
{
    public async Task ExecuteAsync(SocketUserMessage message)
    {
        SocketGuildChannel channel = message.Channel as SocketGuildChannel;

        if (channel is not SocketThreadChannel threadChannel)
        {
            return;
        }

        if (threadChannel.ParentChannel.Id != Channels.FeedbackSuggestions)
        {
            return;
        }

        if (message.CreatedAt == message.Channel.CreatedAt) 
        {
            await message.AddReactionAsync(new Emoji("👍"));
            await message.AddReactionAsync(new Emoji("👎"));
        }
    }
}