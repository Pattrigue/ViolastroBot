using Discord.Commands;
using ViolastroBot.Features.Contests;

namespace ViolastroBot.Features.Commands;

[Name("Contest")]
public sealed class ContestSubmitModule(ContestSubmissions contestSubmissions) : ModuleBase<SocketCommandContext>
{
    public const string CommandName = "submit";

    [Command(CommandName)]
    [Summary("Submit your message to a currently active contest.")]
    public async Task SubmitAsync()
    {
        if (!contestSubmissions.IsContestChannel(Context.Channel.Id))
        {
            await Context.Channel.SendMessageAsync("Bwagh! This ain't no contest submission channel!");
            return;
        }

        var message = Context.Message;
        await contestSubmissions.ProcessAsync(message);
    }
}
