using Discord.Commands;

namespace ViolastroBot.Features.Commands.Contests;

[Name("Contest")]
public sealed class ContestSubmitModule(ContestSubmission submission) : ModuleBase<SocketCommandContext>
{
    private const string CommandName = "submit";

    [Command(CommandName)]
    [Summary("Submit your message to a currently active contest.")]
    public async Task SubmitAsync()
    {
        if (!submission.IsContestChannel(Context.Channel.Id))
        {
            await Context.Channel.SendMessageAsync("Bwagh! This ain't no contest submission channel!");
            return;
        }

        var message = Context.Message;
        await submission.ProcessAsync(message);
    }
}
