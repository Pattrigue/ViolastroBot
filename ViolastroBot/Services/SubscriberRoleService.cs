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
        
        SocketGuild guild = _client.GetGuild(Guilds.SemagGames);
        IMessage message = await guild.GetTextChannel(Channels.RoleInfo).GetMessageAsync(MessageId);
        
        await guild.DownloadUsersAsync();
        
        foreach (IEmote emote in message.Reactions.Keys)
        {
            IEnumerable<IUser> users = await message.GetReactionUsersAsync(emote, int.MaxValue).FlattenAsync();
            
            foreach (IUser user in users)
            {
                SocketGuildUser guildUser = guild.GetUser(user.Id);

                if (guildUser == null)
                {
                    continue;
                }
                
                if (guildUser.Roles.All(role => role.Id != Roles.Subscriber))
                {
                    await _logger.LogMessageAsync($"Adding subscriber role to {guildUser.Mention} during startup because they reacted to the role info message and did not have the subscriber role.");
                    await guildUser.AddRoleAsync(_client.GetGuild(Guilds.SemagGames).GetRole(Roles.Subscriber));
                }
            }
        }
    }

    private async Task OnReactionAdded(
        Cacheable<IUserMessage, ulong> cacheable, 
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction)
    {
        if (reaction.MessageId != MessageId)
        {
            return;
        }
        
        SocketGuildUser user = _client.GetGuild(Guilds.SemagGames).GetUser(reaction.UserId);
        
        if (user.Roles.Any(role => role.Id == Roles.Subscriber))
        {
            return;
        }
       
        await _logger.LogMessageAsync($"Adding subscriber role to {user.Mention}.");
        await user.AddRoleAsync(_client.GetGuild(Guilds.SemagGames).GetRole(Roles.Subscriber));
    }
    
    private async Task OnReactionRemoved(
        Cacheable<IUserMessage, ulong> cacheable,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction)
    {
        if (reaction.MessageId != MessageId)
        {
            return;
        }
        
        SocketGuildUser user = _client.GetGuild(Guilds.SemagGames).GetUser(reaction.UserId);
        
        if (user.Roles.All(role => role.Id != Roles.Subscriber))
        {
            return;
        }
        
        await _logger.LogMessageAsync($"Removing subscriber role from {user.Mention}.");
        await user.RemoveRoleAsync(_client.GetGuild(Guilds.SemagGames).GetRole(Roles.Subscriber));
    }
}