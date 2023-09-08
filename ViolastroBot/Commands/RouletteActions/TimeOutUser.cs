using Discord.WebSocket;

namespace ViolastroBot.Commands.RouletteActions;

/// <summary>
/// Mutes the user for the specified duration.
/// </summary>
[RouletteActionTier(RouletteActionTier.Rare)]
public sealed class TimeOutUser : RouletteAction
{
    public TimeOutUser(IServiceProvider services) : base(services) { }
    
    protected override async Task ExecuteAsync()
    {
        const int muteDurationInMinutes = 1;
        
        SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
        
        await user.SetTimeOutAsync(TimeSpan.FromMinutes(muteDurationInMinutes));
        await ReplyAsync($"Looks like {user.Mention} is out of the game for {muteDurationInMinutes} minutes! Bwehehe!!");
    }
}