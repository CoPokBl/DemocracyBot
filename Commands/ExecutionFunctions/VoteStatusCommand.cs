using DemocracyBot.Data.Schemas;
using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class VoteStatusCommand : ICommandExecutionHandler {
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
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
            Task<IUser> userTask = GetUserSync(kv.Key, client);
            userTask.Wait();
            IUser? user = userTask.Result;
            if (user == null) {
                Logger.Error("User with id '" + kv.Key + "' not found");
                IUser? peopleInServer = client.GetUserAsync(kv.Key).GetAwaiter().GetResult();
                Logger.Error(peopleInServer == null ? "Async failed aswell" : "Async succeeded");
                return current;
            }
            return current + $"{user.Mention}: {kv.Value} votes\n";
        });

        await cmd.RespondWithEmbedAsync(
            $"Current Vote Status (Vote ends {TimestampTag.FromDateTime(DateTime.FromBinary(poll.PollEnd).ToLocalTime(), TimestampTagStyles.Relative)})", 
            message == "" ? "No Votes Have Been Cast" : message);
    }
    
    private static async Task<IUser> GetUserSync(ulong userId, DiscordSocketClient client) {
        return await client.GetUserAsync(userId);
    }
}