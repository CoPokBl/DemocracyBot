using Discord;

namespace DemocracyBot.Commands; 

public class SlashCommandArgument {
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Required { get; set; }
    public ApplicationCommandOptionType Type { get; set; }
    
    public SlashCommandArgument(string name, string description, bool required, ApplicationCommandOptionType type) {
        Name = name;
        Description = description;
        Required = required;
        Type = type;
    }
}