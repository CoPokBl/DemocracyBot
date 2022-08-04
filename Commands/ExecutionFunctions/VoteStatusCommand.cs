using DemocracyBot.Data.Schemas;
using Discord.WebSocket;
using GeneralPurposeLib;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class VoteStatusCommand : ICommandExecutionHandler {
    public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        Poll? poll = Program.StorageService.GetCurrentPoll();

        if (poll == null) {
            await cmd.RespondWithEmbedAsync("Error", "There is no current poll running", ResponseType.Error);
            return;
        }
        
        Dictionary<ulong, int> votesCount = poll.GetVotesCount();
        foreach (KeyValuePair<ulong, int> voteCount in votesCount) {
            Logger.Debug("Votecount: " + voteCount.Key + " " + voteCount.Value);
        }
        
        string message = votesCount.Aggregate("", (current, kv) => {
            SocketUser user = client.GetUser(kv.Key);
            if (user == null) {
                Logger.Error("User with id " + kv.Key + " not found");
                return current;
            }
            return current + $"{client.GetUser(kv.Key).Mention}: {kv.Value} votes\n";
        });

        await cmd.RespondWithEmbedAsync("Current Vote Status", message == "" ? "No Votes Have Been Cast" : message);
    }
}