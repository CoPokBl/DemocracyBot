using System.Diagnostics;
using DemocracyBot.Data.Schemas;
using Discord.WebSocket;
using SimpleDiscordNet.Commands;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class ResignCommand {
    
    [SlashCommand("president-resign", "Resign from the presidency")]
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        Term? term = Program.StorageService.GetCurrentTerm();
        Debug.Assert(term != null);
        if (term.PresidentId != cmd.User.Id) {
            await cmd.RespondWithEmbedAsync(
                "Presidency", 
                "You are not the president and therefore cannot resign.",
                ResponseType.Error);
            return;
        }
        if (Program.StorageService.IsPollRunning()) {
            await cmd.RespondWithEmbedAsync(
                "Presidency", 
                "There is already an election running for a new president.",
                ResponseType.Error);
            return;
        }
        
        Utils.Announce(client, $"{cmd.User.Mention} has resigned from the presidency.");
        Utils.TriggerElection(client);
        await cmd.RespondWithEmbedAsync("Adios", "You have resigned as president.", ResponseType.Success);
    }
    
    
}
