namespace DemocracyBot.Data.Schemas; 

public class Term {

    /// <summary>
    /// The ID of the term
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// The ID of the current president
    /// </summary>
    public ulong PresidentId { get; set; }
    
    /// <summary>
    /// When the term started
    /// </summary>
    public DateTime TermStart { get; set; }
    
    /// <summary>
    /// When the term ends
    /// </summary>
    public DateTime TermEnd { get; set; }
}