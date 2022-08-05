using DemocracyBot.Data.Schemas;
using Discord;
using Discord.WebSocket;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class TermStatusCommand : ICommandExecutionHandler {
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        Term? term = Program.StorageService.GetCurrentTerm();
        if (term.IsNull()) {
            await cmd.RespondWithEmbedAsync("Error", "No term is currently in progress.", ResponseType.Error);
            return;
        }
        await cmd.RespondWithEmbedAsync(
            "Term Status", 
            $"President: {(await client.GetUserAsync(term!.PresidentId)).Mention}\n" +
            $"Term Started {TimestampTag.FromDateTime(DateTime.FromBinary(term.TermStart), TimestampTagStyles.Relative)}\n" +
            $"Term Ends {TimestampTag.FromDateTime(DateTime.FromBinary(term.TermEnd), TimestampTagStyles.Relative)}\n" +
            $"Rioting: {term.RiotVotes.Count}", 
            ResponseType.Success);
    }
}