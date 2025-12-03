using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace ViolastroBot.Features.Commands.Contests;

[Name("Contest")]
public sealed class ContestSubmitModule(ContestSubmission submission) : ModuleBase<SocketCommandContext>
{
    private const string CommandName = "submit";

    [Command(CommandName)]
    [Summary("Submit your message to the contest.")]
    public async Task SubmitAsync()
    {
        if (!submission.IsContestChannel(Context.Channel.Id))
        {
            await Context.Message.DeleteAsync();
            await Context.User.SendMessageAsync(
                "Bwagh! Y'all need to use the `!submit` command in the contest submissions channel to submit your message!"
            );
            return;
        }

        var message = (SocketUserMessage)Context.Message;
        await submission.ProcessAsync(message);
    }
}
