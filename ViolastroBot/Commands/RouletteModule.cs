using System.Reflection;
using Discord.Commands;
using ViolastroBot.Commands.Preconditions;
using ViolastroBot.Commands.RouletteActions;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Commands;

public sealed class RouletteModule : ModuleBase<SocketCommandContext>
{
    private readonly IServiceProvider _services;

    private static readonly string[] Responses = 
    {
        "Deleting the server in 5 minutes...",
        "I'm feeling full of beans!",
        "We think it is good, but we design things on certaim things.",
        "public static void main string args",
        "I HATE USING GREEN TO THE SPIKES",
        "I Can Fix you.",
        "it requires... RNG?????????????????????????????????",
        "Would U need sign a deal???.",
        "( yep, DAILY! ]"
    };

    public RouletteModule(IServiceProvider services)
    {
        _services = services;
    }
    
    [Command("roulette")]
    [RequireRole(Roles.Moderator)]
    public Task PlayRoulette()
    {
        Random random = new Random();

        if (random.Next(0, 100) >= 5)
        {
            return SelectRandomResponse();
        }

        Type[] types = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(RouletteAction)))
            .ToArray();

        Type actionType = types[random.Next(types.Length)];
        RouletteAction actionInstance = (RouletteAction)Activator.CreateInstance(actionType, _services);

        Console.WriteLine($"Executing {actionType.Name}...");
        
        // Execute the action
        actionInstance?.ExecuteAsync(Context);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Selects a random response from the list of responses and sends it to the channel.
    /// </summary>
    private async Task SelectRandomResponse()
    {
        string response = Responses[new Random().Next(0, Responses.Length)];

        await ReplyAsync(response);
    }
}