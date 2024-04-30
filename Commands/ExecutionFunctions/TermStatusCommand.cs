using System.Diagnostics;
using DemocracyBot.Data.Schemas;
using Discord;
using Discord.WebSocket;
using SimpleDiscordNet.Commands;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class TermStatusCommand {
    
    [SlashCommand("term-status", "Display information about the current term")]
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        Term? term = Program.StorageService.GetCurrentTerm();
        Debug.Assert(term != null);

        string president = term.PresidentId == 0 ? "None" : (await client.GetUserAsync(term!.PresidentId)).Mention;
        int rioters = Program.StorageService.CountRioters();
        
        await cmd.RespondWithEmbedAsync(
            "Presidency", 
            $"President: {president}\n" +
            $"Term Started {TimestampTag.FromDateTime(term.TermStart, TimestampTagStyles.Relative)}\n" +
            $"Term Ends {TimestampTag.FromDateTime(term.TermEnd, TimestampTagStyles.Relative)}\n" +
            $"Rioting: {rioters}/{Program.StorageService.GetRequiredRioters()}", 
            ResponseType.Success);
    }
}