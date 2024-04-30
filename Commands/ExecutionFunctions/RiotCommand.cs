using System.Diagnostics;
using DemocracyBot.Data.Schemas;
using Discord.WebSocket;
using SimpleDiscordNet.Commands;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class RiotCommand {
    
    [SlashCommand("riot", "Rise up against the current president to overthrow them")]
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        if (!cmd.User.IsCitizen()) {
            await cmd.RespondWithEmbedAsync("Rebellion", "You must be a citizen to riot.", ResponseType.Error);
            return;
        }
        
        Term? term = Program.StorageService.GetCurrentTerm();
        Debug.Assert(term != null);
        if (Program.StorageService.IsPollRunning()) {
            await cmd.RespondWithEmbedAsync("Rebellion", "You cannot riot while there is an election underway.", ResponseType.Error);
            return;
        }

        if (Program.StorageService.IsRioting(cmd.User.Id)) {
            Program.StorageService.RevokeRiot(cmd.User.Id);
            await cmd.RespondWithEmbedAsync("Rebellion", "You have removed your request to overthrow the president.",
                ResponseType.Success);
            return;
        }

        Program.StorageService.RegisterRiot(cmd.User.Id);
        
        int rioters = Program.StorageService.CountRioters();
        int required = Program.StorageService.GetRequiredRioters();
        
        await cmd.RespondWithEmbedAsync("Rebellion", $"You have voted to overthrow the president. ({rioters}/{required})", ResponseType.Success);

        if (rioters < required) {  // Check if the vote is successful
            return;
        }
        
        Utils.Announce(client, "The president has been overthrown!");
        Utils.TriggerElection(client);
            
        SocketGuildUser president = client.GetGuild(Utils.Pucv("server_id")).GetUser(term.PresidentId);
        ulong roleId = Utils.Pucv("president_role_id");
        if (president != null && president.Roles.Any(r => r.Id == roleId)) {
            await president.RemoveRoleAsync(roleId);
        }
    }
}