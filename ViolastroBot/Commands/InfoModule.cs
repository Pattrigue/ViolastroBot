using Discord.Commands;

namespace ViolastroBot.Commands;

public sealed class InfoModule : ModuleBase<SocketCommandContext>
{
    // ~say hello world -> hello world
    [Command("say")]
    [Summary("Echoes a message.")]
    public Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
    {
        return ReplyAsync(echo);
    }
}