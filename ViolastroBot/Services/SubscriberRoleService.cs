using Discord;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Services.Logging;

namespace ViolastroBot.Services;

public sealed class SubscriberRoleService : ServiceBase
{
    private const ulong MessageId = 786616576371720203;

    private readonly DiscordSocketClient _client;
    private readonly ILoggingService _logger;

    public SubscriberRoleService(DiscordSocketClient client, ILoggingService logger)
    {
        _client = client;
        _logger = logger;
        _client.Ready += OnReady;
        _client.ReactionAdded += OnReactionAdded;
        _client.ReactionRemoved += OnReactionRemoved;
    }

    private async Task OnReady()
    {
        Console.WriteLine($"Checking the role info message ({MessageId}) for users that need the subscriber role...");

        var guild = _client.GetGuild(Guilds.SemagGames);
        var subscriberRole = _client.GetGuild(Guilds.SemagGames).GetRole(Roles.Subscriber);
        var message = await guild.GetTextChannel(Channels.RoleInfo).GetMessageAsync(MessageId);

        await guild.DownloadUsersAsync();

        foreach (var emote in message.Reactions.Keys)
        {
            var users = await message.GetReactionUsersAsync(emote, int.MaxValue).FlattenAsync();

            foreach (var user in users)
            {
                var guildUser = guild.GetUser(user.Id);

                if (guildUser == null)
                {
                    continue;
                }

                if (guildUser.Roles.All(role => role.Id != Roles.Subscriber))
                {
                    await _logger.LogMessageAsync(
                        $"Adding subscriber role to {guildUser.Mention} during startup because they reacted to the role info message and did not have the subscriber role."
                    );
                    await guildUser.AddRoleAsync(subscriberRole);
                }
            }
        }
    }

    private async Task OnReactionAdded(
        Cacheable<IUserMessage, ulong> cacheable,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction
    )
    {
        if (reaction.MessageId != MessageId)
        {
            return;
        }

        var guild = _client.GetGuild(Guilds.SemagGames);
        var user = guild.GetUser(reaction.UserId);

        if (user == null || user.Roles.Any(role => role.Id == Roles.Subscriber))
        {
            return;
        }

        var subscriberRole = guild.GetRole(Roles.Subscriber);

        await _logger.LogMessageAsync($"Adding subscriber role to {user.Mention}.");
        await user.AddRoleAsync(subscriberRole);
    }

    private async Task OnReactionRemoved(
        Cacheable<IUserMessage, ulong> cacheable,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction
    )
    {
        if (reaction.MessageId != MessageId)
        {
            return;
        }

        var guild = _client.GetGuild(Guilds.SemagGames);
        var user = guild.GetUser(reaction.UserId);

        if (user == null || user.Roles.All(role => role.Id != Roles.Subscriber))
        {
            return;
        }

        var subscriberRole = guild.GetRole(Roles.Subscriber);

        await _logger.LogMessageAsync($"Removing subscriber role from {user.Mention}.");
        await user.RemoveRoleAsync(subscriberRole);
    }
}
