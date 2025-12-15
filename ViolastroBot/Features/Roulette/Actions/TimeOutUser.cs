using Discord.Commands;
using ViolastroBot.Logging;

namespace ViolastroBot.Features.Roulette.Actions;

/// <summary>
/// Mutes the user for the specified duration.
/// </summary>
[RouletteActionTier(RouletteActionTier.Rare)]
public sealed class TimeOutUser(ILoggingService logger) : RouletteAction
{
    public override async Task ExecuteAsync(SocketCommandContext context)
    {
        const int muteDurationInMinutes = 1;
        const string minutes = muteDurationInMinutes == 1 ? "minute" : "minutes";

        var user = context.Guild.GetUser(context.User.Id);

        try
        {
            await user.SetTimeOutAsync(TimeSpan.FromMinutes(muteDurationInMinutes));
            await ReplyAsync(
                context,
                $"Looks like {user.Mention} needs to shut their gob for {muteDurationInMinutes} {minutes}! Bwehehe!!"
            );
        }
        catch (Exception ex)
        {
            await logger.LogMessageAsync(
                $"I couldn't mute {user.Mention}!{Environment.NewLine}Exception: {ex.Message}"
            );
        }
    }
}
