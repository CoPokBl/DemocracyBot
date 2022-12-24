using DemocracyBot.Data.Schemas;
using GeneralPurposeLib;
using Newtonsoft.Json;

namespace DemocracyBot.Data.Storage; 

public class FileStorageService : IStorageService {
    
    private Poll? _currentPoll;
    private Term? _currentTerm;
    
    public void Init() {
        if (!File.Exists("data.json")) {
            return;
        }
        string json = File.ReadAllText("data.json");
        (Poll?, Term?) data = JsonConvert.DeserializeObject<(Poll?, Term?)>(json);
        _currentPoll = data.Item1;
        _currentTerm = data.Item2;

        if (_currentPoll == null) return;
        foreach (KeyValuePair<ulong, ulong> vote in _currentPoll.Votes) {
            Logger.Debug("Vote: " + vote.Key + " " + vote.Value);
        }
    }

    public void Deinit() {
        Save();
    }

    public void Save() {
        //Logger.Debug("Saving...");
        string json = JsonConvert.SerializeObject((_currentPoll, _currentTerm));
        File.WriteAllText("data.json", json);
    }

    public void RegisterVote(ulong user, ulong vote) {
        if (_currentPoll == null) throw new Exception("No poll is currently running");
        _currentPoll.Votes.Add(user, vote);
    }

    public Term? GetCurrentTerm() {
        return _currentTerm;
    }

    public void SetCurrentTerm(Term term) {
        _currentTerm = term;
    }

    public Poll? GetCurrentPoll() {
        return _currentPoll;
    }

    public void StartNewPoll() {
        _currentPoll = new Poll {
            Votes = new Dictionary<ulong, ulong>(),
            PollStart = DateTime.UtcNow.ToBinary(),
            PollEnd = DateTime.UtcNow.AddHours(Convert.ToDouble(Program.Config!["poll_time"])).ToBinary()
        };
    }

    public void EndPoll(out ulong winner, out int votes) {
        if (_currentPoll == null) throw new Exception("No poll is currently running");
        winner = _currentPoll.GetWinner();
        votes = _currentPoll.GetVotesCount()[winner];
        _currentPoll = null;
    }

    public void NullifyPoll() {
        _currentPoll = null;
    }
}
