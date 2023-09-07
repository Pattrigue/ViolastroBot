using Discord.Commands;
using ViolastroBot.Commands.Preconditions;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Extensions;
using ViolastroBot.RandomWords;
using ViolastroBot.Services.Logging;

namespace ViolastroBot.Commands;

public sealed class RouletteModule : ModuleBase<SocketCommandContext>
{
    private ILoggingService _logger;
    
    public RouletteModule(ILoggingService logger)
    {
        _logger = logger;
    }
    
    [Command("roulette")]
    [RequireRole(Roles.Moderator)]
    public Task PlayRoulette()
    {
        return SetUsernameToRandomWords();
    }

    private async Task SetUsernameToRandomWords()
    {
        List<string> words = new WordRandomizer().GetRandomWords(1, 3);
        string name = string.Join(" ", words).CapitalizeFirstCharacter();
    
        try
        {
            await Context.Guild.GetUser(Context.User.Id).ModifyAsync(properties => properties.Nickname = name);
            await ReplyAsync($"Bwehehe!! Ya name is now \"{name}\"!!");
        }
        catch (Discord.Net.HttpException ex)
        {
            await _logger.LogMessageAsync($"Failed to change {Context.User.Mention}'s name to \"{name}\".{Environment.NewLine}Exception: {ex.Message}");
        }
        catch (Exception ex) 
        {
            await _logger.LogMessageAsync($"An unexpected error occurred: {ex.Message}");
        }
    }
}