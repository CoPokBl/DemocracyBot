using DemocracyBot.Data.Schemas;
using Discord.WebSocket;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class VoteStatusCommand : ICommandExecutionHandler {
    public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        Poll poll = Program.StorageService.GetCurrentPoll();

        if (poll == null) {
            await cmd.RespondWithEmbedAsync("Error", "There is no current poll running", ResponseType.Error);
            return;
        }
        
        Dictionary<ulong, int> votesCount = poll.GetVotesCount();
        
        string message = "";

        foreach (KeyValuePair<ulong, int> kv in votesCount) {
            message += $"{kv.Key}: {kv.Value}\n";
        }

        await cmd.RespondWithEmbedAsync("Current Vote Status", message);
    }
}