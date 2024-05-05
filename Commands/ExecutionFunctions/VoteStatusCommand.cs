using DemocracyBot.Data.Schemas;
using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;
using SimpleDiscordNet.Commands;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class VoteStatusCommand {
    
    [SlashCommand("vote-status", "Check the current state of the election, including vote counts")]
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        Poll? poll = Program.StorageService.GetPoll();
        
        if (poll == null) {
            await cmd.RespondWithEmbedAsync("Election", "There is no current poll running", ResponseType.Error);
            return;
        }
        
        string message = poll.Votes.Aggregate("", (current, kv) => {
            Task<IUser> userTask = GetUserSync(kv.Key, client);
            userTask.Wait();
            
            IUser? user = userTask.Result;
            if (user != null!) return current + $"{user.Mention}: {kv.Value} votes\n";
            
            Logger.Error("User with id '" + kv.Key + "' not found");
            return current;
        });

        await cmd.RespondWithEmbedAsync(
            $"Current Election Standings (Election ends {TimestampTag.FromDateTime(poll.PollEnd, TimestampTagStyles.Relative)})", 
            message == "" ? "No votes have been cast" : message);
    }
    
    private static async Task<IUser> GetUserSync(ulong userId, DiscordSocketClient client) {
        return await client.GetUserAsync(userId);
    }
}