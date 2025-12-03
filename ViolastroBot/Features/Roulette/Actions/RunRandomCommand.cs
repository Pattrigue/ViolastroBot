using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace ViolastroBot.Features.Roulette.Actions;

/// <summary>
/// Runs a random command that has no required roles and no required parameters.
/// </summary>
[RouletteActionTier(RouletteActionTier.Common)]
public sealed class RunRandomCommand(IServiceProvider services) : RouletteAction(services)
{
    private readonly CommandService _commands = services.GetRequiredService<CommandService>();

    protected override async Task ExecuteAsync()
    {
        var commands = _commands
            .Commands.Where(command =>
            {
                return command.Preconditions.All(precondition => precondition is not RequireRoleAttribute)
                    && (command.Parameters.Count == 0 || command.Parameters.All(p => p.IsOptional));
            })
            .ToList();

        if (commands.Count == 0)
        {
            Console.WriteLine("No commands found.");
            return;
        }

        var randomCommand = commands.ElementAt(new Random().Next(0, commands.Count));

        await ReplyAsync($"!{randomCommand.Name}");
        await _commands.ExecuteAsync(Context, randomCommand.Name, Services);
    }
}
