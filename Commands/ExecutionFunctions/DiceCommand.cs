using Discord.WebSocket;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class DiceCommand : ICommandExecutionHandler {
     public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        int sides = (int) (cmd.GetArgument<long?>("sides") ?? 6);
        int rolls = (int) (cmd.GetArgument<long?>("rolls") ?? 1);
        switch (sides) {
            case > 256:
                await cmd.RespondWithUsageAsync("Dice cannot have more than 256 sides.");
                return;
            case < 1:
                await cmd.RespondWithUsageAsync("Dice cannot have less than 1 side.");
                return;
        }
        if (rolls > 100) {
            await cmd.RespondWithUsageAsync("I'm sorry but I'm not spending that much time rolling dice. (Max 100 rolls)");
            return;
        }
        
        string response = "";
        for (int i = 0; i < rolls; i++) {
            response += $"{Program.Random.Next(1, sides)}, ";
        }
        response = response.TrimEnd(',', ' ');
        await cmd.RespondWithEmbedAsync("Dice Roll", response); 
    }
}