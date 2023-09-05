using Discord.Commands;
using Discord.WebSocket;

namespace ViolastroBot.Commands.Preconditions;

public sealed class RequireRoleAttribute : PreconditionAttribute
{
    public ulong RoleId { get; }

    public RequireRoleAttribute(ulong id)
    {
        RoleId = id;
    }

    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        if (context.User is SocketGuildUser guildUser && guildUser.Roles.Any(role => role.Id == RoleId))
        {
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        return Task.FromResult(PreconditionResult.FromError("You must have the required role to run this command."));
    }
}