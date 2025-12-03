using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.Services.Logging;

namespace ViolastroBot.Commands.Roulette.Actions;

/// <summary>
/// Mutes the user for the specified duration.
/// </summary>
[RouletteActionTier(RouletteActionTier.Rare)]
public sealed class TimeOutUser : RouletteAction
{
    private readonly ILoggingService _logger;

    public TimeOutUser(IServiceProvider services) : base(services)
    {
        _logger = services.GetRequiredService<ILoggingService>();
    }
    
    protected override async Task ExecuteAsync()
    {
        const int muteDurationInMinutes = 1;
        const string minutes = muteDurationInMinutes == 1 ? "minute" : "minutes";
        
        var user = Context.Guild.GetUser(Context.User.Id);

        try
        {
            await user.SetTimeOutAsync(TimeSpan.FromMinutes(muteDurationInMinutes));
            await ReplyAsync($"Looks like {user.Mention} needs to shut their gob for {muteDurationInMinutes} {minutes}! Bwehehe!!");
        }
        catch (Exception ex)
        {
            await _logger.LogMessageAsync($"I couldn't mute {user.Mention}!{Environment.NewLine}Exception: {ex.Message}");
        }
    }
}