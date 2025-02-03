using DemocracyBot.Data.Schemas;

namespace DemocracyBot.Data.Storage; 

public interface IStorageService {
    void Init();
    void Deinit();

    void RegisterVote(ulong user, ulong vote);
    void RegisterCitizenshipVote(ulong user, ulong target);
    void RevokeCitizenshipVote(ulong user, ulong target);
    void AddCitizen(ulong user);
    void RemoveCitizen(ulong user);
    void CreateTerm(ulong president, DateTime start, DateTime end);
    void RegisterRiot(ulong user);
    void RevokeRiot(ulong user);
    void SetCurrentTermEnd(DateTime end);
    void SetState(string state);

    Dictionary<ulong, int> TallyVotes();
    int CountCitizenshipVotesFor(ulong user);
    int CountCitizens();
    bool IsCitizen(ulong user);
    int CountRioters();
    Term? GetCurrentTerm();
    bool IsRioting(ulong user);
    string GetState();

    void ClearVotes();
    void ClearRioters();
}
