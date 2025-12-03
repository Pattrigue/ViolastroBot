using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.Extensions;
using ViolastroBot.Features.RandomWords;
using ViolastroBot.Logging;

namespace ViolastroBot.Features.Commands.Roulette.Actions;

/// <summary>
/// Sets the user's nickname to a random name.
/// </summary>
[RouletteActionTier(RouletteActionTier.Uncommon)]
public sealed class SetUsernameToRandomWords(IServiceProvider services) : RouletteAction(services)
{
    private readonly ILoggingService _logger = services.GetRequiredService<ILoggingService>();

    protected override async Task ExecuteAsync()
    {
        var words = new WordRandomizer().GetRandomWords(1, 3);
        var name = string.Join(" ", words).CapitalizeFirstCharacterInEachWord();

        try
        {
            var displayName = Context.User.GlobalName ?? Context.User.Username;

            await Context
                .Guild.GetUser(Context.User.Id)
                .ModifyAsync(properties => properties.Nickname = $"{name} ({displayName})");
            await ReplyAsync($"Bwehehe!! Ya name is now \"{name}\"!!");
        }
        catch (Discord.Net.HttpException ex)
        {
            await _logger.LogMessageAsync(
                $"Failed to change {Context.User.Mention}'s name to \"{name}\".{Environment.NewLine}Exception: {ex.Message}"
            );
        }
        catch (Exception ex)
        {
            await _logger.LogMessageAsync($"An unexpected error occurred: {ex.Message}");
        }
    }
}
