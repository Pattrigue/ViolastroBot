using System.Text;
using Discord.WebSocket;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Commands.RouletteActions;

/// <summary>
/// Assigns a new role to the user, and removes it from the user who currently has it.
/// </summary>
public sealed class AssignNewRole : RouletteAction
{
    public AssignNewRole(IServiceProvider services) : base(services) { }
    
    protected override async Task ExecuteAsync()
    {
        SocketRole role = Context.Guild.GetRole(Roles.NewRole);
        
        SocketGuildUser userWithRole = Context.Guild.Users.FirstOrDefault(user =>
            user.Roles.Any(userRole => userRole.Id == Roles.NewRole));
        
        StringBuilder reply = new();
        
        if (userWithRole != null)
        {
            reply.AppendLine($"That means {userWithRole.Mention} no longer has it - too bad!");
            await userWithRole.RemoveRoleAsync(Roles.NewRole);
        }
        
        SocketGuildUser userToReceiveRole = Context.Guild.GetUser(Context.User.Id);

        await userToReceiveRole.AddRoleAsync(Roles.NewRole);
        reply.Insert(0, $"Bwehehe!! {userToReceiveRole.Mention} now has the {role.Mention} role! ");
        
        await ReplyAsync(reply.ToString());
    }
}