using Discord.WebSocket;

namespace SerbleBot.Commands.ExecutionFunctions; 

public class RockPaperScissorsCommand : ICommandExecutionHandler {

    public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        // Get choice
        string choice = cmd.GetArgument<string>("choice")!;
        
        string reply;
        switch (choice.ToLower()) {
            case "r":
            case "rock":
                reply = "Paper!";
                break;
                
            case "p":
            case "paper":
                reply = "Scissors!";
                break;
                
            case "s":
            case "scissors":
                reply = "Rock!";
                break;
                
            default:
                await cmd.RespondWithUsageAsync("Syntax: !rockpaperscissors <rock/paper/scissors>");
                return;
        }

        await cmd.RespondWithEmbedAsync("Rock Paper Scissors", reply + "\n**I Won!**");
    }
}