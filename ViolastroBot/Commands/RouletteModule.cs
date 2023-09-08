using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.Commands.Preconditions;
using ViolastroBot.DiscordServerConfiguration;
using ViolastroBot.Extensions;
using ViolastroBot.RandomWords;
using ViolastroBot.Services.Logging;

namespace ViolastroBot.Commands;

public sealed class RouletteModule : ModuleBase<SocketCommandContext>
{
    private readonly IServiceProvider _services;
    private readonly CommandService _commands;
    private readonly ILoggingService _logger;
    
    public RouletteModule(IServiceProvider services)
    {
        _services = services;
        _logger = services.GetRequiredService<ILoggingService>();
        _commands = services.GetRequiredService<CommandService>();
    }
    
    [Command("roulette")]
    [RequireRole(Roles.Moderator)]
    public Task PlayRoulette()
    {
        return RunRandomCommand();
    }

    /// <summary>
    /// Sets the user's nickname to a random name.
    /// </summary>
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

    /// <summary>
    /// Runs a random command that has no required roles and no required parameters.
    /// </summary>
    private async Task RunRandomCommand()
    {
        IEnumerable<CommandInfo> commands = _commands.Commands.Where(command =>
        {
            return command.Preconditions.All(precondition => precondition is not RequireRoleAttribute) 
                   && (command.Parameters.Count == 0 || command.Parameters.All(p => p.IsOptional));
        }).ToList();

        CommandInfo randomCommand = commands.ElementAt(new Random().Next(0, commands.Count()));
        
        await _commands.ExecuteAsync(Context, randomCommand.Name, _services);
    }
}