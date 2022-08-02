namespace DemocracyBot.Data.Schemas; 

public class Poll {
    
    /// <summary>
    /// (User who voted, who they voted for)
    /// </summary>
    public Dictionary<ulong, ulong> Votes { get; set; }

    /// <summary>
    /// When the poll started
    /// </summary>
    public long PollStart { get; set; }
    
    /// <summary>
    /// When the election will be called
    /// </summary>
    public long PollEnd { get; set; }
    
    public ulong GetWinner() {
        int max = GetVotesCount().Max(kvp => kvp.Value);
        return GetVotesCount().First(kvp => kvp.Value == max).Key;
    }
    
    public Dictionary<ulong, int> GetVotesCount() {
        Dictionary<ulong, int> votes = new();
        foreach (ulong vote in Votes.Values) {
            if (!votes.ContainsKey(vote)) {
                votes.Add(vote, 0);
            }
            votes[vote]++;
        }
        return votes;
    }

    public bool HasVoted(ulong user) {
        return Votes.ContainsKey(user);
    }
    
}