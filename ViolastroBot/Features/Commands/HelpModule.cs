using Discord;
using Discord.Commands;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Features.Commands;

[Name("Help")]
public sealed class HelpModule(CommandService commands) : ModuleBase<SocketCommandContext>
{
    [Command("help")]
    [Summary("Displays a list of commands.")]
    public Task DisplayCommands()
    {
        EmbedBuilder embedBuilder = new();
        embedBuilder.WithTitle("ViolastroBot Commands");
        embedBuilder.WithDescription("Ya best try and use some o' these commands, bwehehe!");
        embedBuilder.WithColor(Color.Blue);

        var isUserModerator = Context.Guild.GetUser(Context.User.Id).Roles.Any(role => role.Id == Roles.Moderator);

        foreach (var module in commands.Modules)
        {
            if (module.Name == nameof(HelpModule))
            {
                continue;
            }

            var description = string.Empty;

            foreach (var command in module.Commands)
            {
                if (HasCommandPermissions(command, isUserModerator))
                {
                    description += $"`{command.Name}` - {command.Summary}\n";
                }
            }

            if (!string.IsNullOrEmpty(description))
            {
                embedBuilder.AddField(module.Name, description);
            }
        }

        return ReplyAsync(embed: embedBuilder.Build());
    }

    private static bool HasCommandPermissions(CommandInfo command, bool isUserModerator)
    {
        foreach (var attribute in command.Preconditions)
        {
            if (attribute is RequireRoleAttribute requireRoleAttribute)
            {
                if (requireRoleAttribute.RoleId == Roles.Moderator && !isUserModerator)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
