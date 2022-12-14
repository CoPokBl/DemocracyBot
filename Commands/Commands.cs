using DemocracyBot.Commands.ExecutionFunctions;
using Discord;

namespace DemocracyBot.Commands;

public static class Commands {

    public static readonly SlashCommand[] SlashCommands = {
        new ("vote", "Vote for the next president!",
            new [] {
                new SlashCommandArgument("president", "The person to vote for president", true, ApplicationCommandOptionType.User)
            },
            new VoteCommand(),
            null,
            false
        ),
        
        new ("vote-status", "Current votes for the next president!",
            Array.Empty<SlashCommandArgument>(),
            new VoteStatusCommand(),
            null,
            false
        ),
        
        new ("riot", "Vote to overthrow the president", 
            Array.Empty<SlashCommandArgument>(),
            new RiotCommand(),
            null,
            false),
        
        new ("term-status", "Gets information about the current term",
            Array.Empty<SlashCommandArgument>(),
            new TermStatusCommand(),
            null,
            false),
        
        new ("president-resign", "Resign as president and trigger election",
            Array.Empty<SlashCommandArgument>(),
            new ResignCommand(),
            null,
            false),
        
        new ("dev-tools", "This is only for the developer",
            new [] {
                new SlashCommandArgument("command", "Dev command", false, ApplicationCommandOptionType.String)
            },
            new DevToolsCommand(),
            null,
            false),
    };

}
