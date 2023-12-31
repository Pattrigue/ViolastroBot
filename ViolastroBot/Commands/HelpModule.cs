﻿using Discord;
using Discord.Commands;
using ViolastroBot.Commands.Preconditions;
using ViolastroBot.DiscordServerConfiguration;

namespace ViolastroBot.Commands;

[Name("Help")]
public sealed class HelpModule : ModuleBase<SocketCommandContext>
{
    private readonly CommandService _commands;
    
    public HelpModule(CommandService commands)
    {
        _commands = commands;
    }
    
    [Command("help")]
    [Summary("Displays a list of commands.")]
    public Task DisplayCommands()
    {
        EmbedBuilder embedBuilder = new();
        embedBuilder.WithTitle("ViolastroBot Commands");
        embedBuilder.WithDescription("Ya best try and use some o' these commands, bwehehe!");
        embedBuilder.WithColor(Color.Blue);

        bool isUserModerator = Context.Guild.GetUser(Context.User.Id).Roles.Any(role => role.Id == Roles.Moderator);
        
        foreach (ModuleInfo module in _commands.Modules)
        {
            if (module.Name == nameof(HelpModule))
            {
                continue;
            }
            
            string description = string.Empty;

            foreach (CommandInfo command in module.Commands)
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
        foreach (PreconditionAttribute attribute in command.Preconditions)
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