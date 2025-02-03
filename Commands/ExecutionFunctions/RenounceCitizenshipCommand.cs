using Discord.WebSocket;
using SimpleDiscordNet.Commands;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class RenounceCitizenshipCommand {
    
    [SlashCommand("renounce-citizenship", "Voluntarily give up your citizenship (This cannot be undone).")]
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        bool isCitizen = Program.StorageService.IsCitizen(cmd.User.Id);
        if (!isCitizen) {
            await cmd.RespondWithEmbedAsync(
                "Citizenship", 
                "You are not currently a citizen and therefore cannot renounce it.",
                ResponseType.Error);
            return;
        }
        
        Program.StorageService.RemoveCitizen(cmd.User.Id);
        
        Utils.Announce(client, $"{cmd.User.Mention} has renounced their citizenship. They are no longer a citizen.");
        await cmd.RespondWithEmbedAsync("Adios", "You have renounced your citizenship.", ResponseType.Success);
    }
}
