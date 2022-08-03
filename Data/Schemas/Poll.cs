namespace DemocracyBot.Data.Schemas; 

public class Poll {
    
    /// <summary>
    /// (User who voted, who they voted for)
    /// </summary>
    public Dictionary<ulong, ulong> Votes { get; set; } = null!;

    /// <summary>
    /// When the poll started
    /// </summary>
    public long PollStart { get; set; }
    
    /// <summary>
    /// When the election will be called
    /// </summary>
    public long PollEnd { get; set; }
    
    /// <summary>
    /// Gets the current winner of the poll
    /// </summary>
    /// <returns>The ID of the winning user</returns>
    /// <exception cref="Exception">When no votes were cast and there is no current president</exception>
    public ulong GetWinner() {
        if (Votes.Count == 0) {
            Term? cTerm = Program.StorageService!.GetCurrentTerm();
            if (cTerm == null) {
                throw new Exception("Cannot select winner because no votes were cast and there is no current president");
            }
            return cTerm.PresidentId;
        }
        int max = GetVotesCount().Max(kvp => kvp.Value);
        return GetVotesCount().First(kvp => kvp.Value == max).Key;
    }
    
    /// <summary>
    /// Gets the number of votes for each user
    /// </summary>
    /// <returns>A dictionary containing the data (userid, votes)</returns>
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

    /// <summary>
    /// Checks if a user has voted in the poll
    /// </summary>
    /// <param name="user">The user to check for</param>
    /// <returns>Whether or not they have voted</returns>
    public bool HasVoted(ulong user) {
        return Votes.ContainsKey(user);
    }
    
}