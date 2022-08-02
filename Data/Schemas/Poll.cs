namespace DemocracyBot.Data.Schemas; 

public class Poll {
    
    /// <summary>
    /// Current votes, (Runner, Vote Count)
    /// </summary>
    public Dictionary<long, int> Votes { get; set; }
    
    /// <summary>
    /// When the poll started
    /// </summary>
    public long PollStart { get; set; }
    
    /// <summary>
    /// When the election will be called
    /// </summary>
    public long PollEnd { get; set; }
    
}