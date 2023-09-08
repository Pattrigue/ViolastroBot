using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using ViolastroBot.Commands.Preconditions;

namespace ViolastroBot.Commands.Roulette.Actions;

/// <summary>
/// Runs a random command that has no required roles and no required parameters.
/// </summary>
[RouletteActionTier(RouletteActionTier.Common)]
public sealed class RunRandomCommand : RouletteAction
{
    private readonly CommandService _commands;
    
    public RunRandomCommand(IServiceProvider services) : base(services)
    {
        _commands = services.GetRequiredService<CommandService>();
    }
    
    protected override async Task ExecuteAsync()
    {
        IEnumerable<CommandInfo> commands = _commands.Commands.Where(command =>
        {
            return command.Preconditions.All(precondition => precondition is not RequireRoleAttribute) &&
                   (command.Parameters.Count == 0 || command.Parameters.All(p => p.IsOptional));
        }).ToList();
        
        if (!commands.Any())
        {
            Console.WriteLine("No commands found.");
            return;
        }

        CommandInfo randomCommand = commands.ElementAt(new Random().Next(0, commands.Count()));
        
        await ReplyAsync($"!{randomCommand.Name}");
        await _commands.ExecuteAsync(Context, randomCommand.Name, Services);
    }
}