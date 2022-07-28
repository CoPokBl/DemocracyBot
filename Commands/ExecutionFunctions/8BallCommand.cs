using Discord.WebSocket;

namespace SerbleBot.Commands.ExecutionFunctions; 

public class EightBallCommand : ICommandExecutionHandler {
    
    private static readonly List<string> Responses = new () {
        // Yes
        "Yes",
        "100%",
        "It is certain",
        "Without a doubt",
        "You may rely on it",
        "Definitely",
            
        // No
        "No",
        "Never",
        "0%",
        "Nah",
        "Tomorrow maybe",
        "Don't rely on it",

        // Maybe
        "Maybe",
        "Perhaps",
        "Not sure",
        "I'm tired"
            
    };
    
    public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        await cmd.RespondWithEmbedAsync("8Ball", Responses[Program.Random.Next(Responses.Count)]);
    }
}