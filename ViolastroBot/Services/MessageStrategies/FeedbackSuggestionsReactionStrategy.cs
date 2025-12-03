using Discord;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Services.MessageStrategies;

public sealed class FeedbackSuggestionsReactionStrategy : IMessageStrategy
{
    public async Task<bool> ExecuteAsync(SocketUserMessage message)
    {
        var channel = message.Channel as SocketGuildChannel;

        if (channel is not SocketThreadChannel threadChannel)
        {
            return false;
        }

        if (threadChannel.ParentChannel.Id != Channels.FeedbackSuggestions)
        {
            return false;
        }

        if (message.CreatedAt != message.Channel.CreatedAt)
        {
            return false;
        }

        await message.AddReactionAsync(new Emoji("👍"));
        await message.AddReactionAsync(new Emoji("👎"));

        return true;
    }
}
