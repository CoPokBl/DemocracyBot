namespace DemocracyBot.Data.Schemas; 

public class Term {
    
    /// <summary>
    /// The ID of the current president
    /// </summary>
    public ulong PresidentId { get; set; }
    
    /// <summary>
    /// When the term started
    /// </summary>
    public long TermStart { get; set; }
    
    /// <summary>
    /// When the term ends
    /// </summary>
    public long TermEnd { get; set; }
    
    /// <summary>
    /// List of people who have voted to overthrow the president
    /// </summary>
    public List<ulong> RiotVotes { get; set; } = null!;
}