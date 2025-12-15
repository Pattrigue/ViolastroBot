using Discord.Commands;

namespace ViolastroBot.Features.Roulette.Actions;

/// <summary>
/// Runs a random command that has no required roles and no required parameters.
/// </summary>
[RouletteActionTier(RouletteActionTier.Common)]
public sealed class RunRandomCommand(CommandService commandService, IServiceProvider services) : RouletteAction
{
    protected override async Task ExecuteAsync()
    {
        var commands = commandService
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
        await commandService.ExecuteAsync(Context, randomCommand.Name, services);
    }
}
