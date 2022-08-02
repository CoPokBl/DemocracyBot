using DemocracyBot.Data.Schemas;

namespace DemocracyBot.Data.Storage; 

public interface IStorageService {
    void Init();
    void Deinit();

    void RegisterVote(long user, long vote);
    
    Term GetCurrentTerm();
    void SetCurrentTerm(Term term);
    
    Poll GetCurrentPoll();
    void StartNewPoll();
    void EndPoll(out long winner, out int votes);
}
