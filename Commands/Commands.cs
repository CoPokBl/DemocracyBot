using Discord;
using SerbleBot.Commands.ExecutionFunctions;

namespace SerbleBot.Commands;

public static class Commands {

    public static readonly SlashCommand[] SlashCommands = {

        new (
            "info",
            "Get information about the bot",
            Array.Empty<SlashCommandArgument>(),
            new InfoCommand(),
            null,
            false),

        new ("shorten-link",
            "Generates a shortened link using link.serble.net",
            new [] {
                new SlashCommandArgument("link", "The link to shorten", true, ApplicationCommandOptionType.String),
                new SlashCommandArgument("name", "The custom text to be in the shortened link", false, ApplicationCommandOptionType.String)
            },
            new LinkShortenCommand(),
            null,
            false
            ),

        new ("rock-paper-scissors", "Play a game of rock paper scissors",
            new [] {
                new SlashCommandArgument("choice", "Rock paper or scissors?", true, ApplicationCommandOptionType.String)
            },
            new RockPaperScissorsCommand(),
            null,
            false
            ),

        new ("8ball", "Ask the magic 8 ball a question",
            new [] {
                new SlashCommandArgument("question", "The question to ask the 8 ball", false, ApplicationCommandOptionType.String)
            },
            new EightBallCommand(),
            null,
            false
            ),

        new ("roll", "Roll dice",
            new [] {
                new SlashCommandArgument("sides", "The number of sides on the dice", false, ApplicationCommandOptionType.Integer),
                new SlashCommandArgument("rolls", "The number of dice to roll", false, ApplicationCommandOptionType.Integer)
            },
            new DiceCommand(),
            null,
            false
            ),

        new ("wheel-of-fortune", "Get a random thing in a list",
            new [] {
                new SlashCommandArgument("item1" , "Item to roll for", true , ApplicationCommandOptionType.String),
                new SlashCommandArgument("item2" , "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item3" , "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item4" , "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item5" , "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item6" , "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item7" , "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item8" , "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item9" , "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item10", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item11", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item12", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item13", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item14", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item15", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item16", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item17", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item18", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item19", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item20", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item21", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item22", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item23", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item24", "Item to roll for", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("item25", "Item to roll for", false, ApplicationCommandOptionType.String)
            },
            new WheelOfFortuneCommand(),
            null,
            false
        ),

        new ("image", "Search Serble Images",
            new [] {
                new SlashCommandArgument("query", "The query to search Serble Images for", true, ApplicationCommandOptionType.String)
            },
            new ImageSearchCommand(),
            null,
            false
            ),

        new ("clear", "Clears Multiple Messages",
            new [] {
                new SlashCommandArgument("count", "The amount of messages to delete", true, ApplicationCommandOptionType.Integer)
            },
            new ClearCommand(),
            GuildPermission.ManageMessages,
            false
        ),

        new ("truth", "Gives a truth prompt",
            Array.Empty<SlashCommandArgument>(),
            new TruthCommand(),
            null,
            false
        ),

        new ("dare", "Gives a dare prompt",
            Array.Empty<SlashCommandArgument>(),
            new DareCommand(),
            null,
            false
        ),

        new ("dad-joke", "Tells a dad joke",
            Array.Empty<SlashCommandArgument>(),
            new DadJokeCommand(),
            null,
            false
        ),

        new ("scan-link", "Scans a link for malicious content",
            new [] {
                new SlashCommandArgument("link", "The link to scan", true, ApplicationCommandOptionType.String)
            },
            new ScanLinkCommand(),
            null,
            false),

        new ("sha256", "Computes a SHA256 hash of the provided text as UTF-8 encoded string",
            new [] {
                new SlashCommandArgument("text", "The data to hash", true, ApplicationCommandOptionType.String)
            }, 
            new Sha256Command(),
            null,
            false),

        new ("embed", "Creates an embed",
            new [] {
                new SlashCommandArgument("title", "The title of the embed", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("body", "The body of the embed", true, ApplicationCommandOptionType.String),
                new SlashCommandArgument("colour", "The colour of the embed", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("footer", "The footer of the embed", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("url", "The url of the embed", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("image-url", "The image of the embed", false, ApplicationCommandOptionType.String),
                new SlashCommandArgument("thumbnail-url", "The thumbnail of the embed", false, ApplicationCommandOptionType.String)
            },
            new EmbedCommand(),
            GuildPermission.ManageGuild,
            false)
    };

}
