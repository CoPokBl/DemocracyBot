using DemocracyBot.Data.Schemas;

namespace DemocracyBot.Data.Storage; 

public interface IStorageService {
    void Init();
    void Deinit();
    void Save();

    void RegisterVote(ulong user, ulong vote);
    
    Term? GetCurrentTerm();
    void SetCurrentTerm(Term term);
    
    Poll? GetCurrentPoll();
    void StartNewPoll();
    void EndPoll(out ulong winner, out int votes);
    void NullifyPoll();
}
