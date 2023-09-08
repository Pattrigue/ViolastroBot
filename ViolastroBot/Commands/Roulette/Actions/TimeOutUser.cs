using Discord.WebSocket;

namespace ViolastroBot.Commands.Roulette.Actions;

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
        const string minutes = muteDurationInMinutes == 1 ? "minute" : "minutes";
        
        SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);

        try
        {
            await user.SetTimeOutAsync(TimeSpan.FromMinutes(muteDurationInMinutes));
            await ReplyAsync($"Looks like {user.Mention} is out of the game for {muteDurationInMinutes} {minutes}! Bwehehe!!");
        }
        catch (Exception ex)
        {
            await ReplyAsync($"I couldn't mute {user.Mention}!{Environment.NewLine}Exception: {ex.Message}");
        }
    }
}