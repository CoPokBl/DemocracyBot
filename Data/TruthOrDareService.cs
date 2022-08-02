using GeneralPurposeLib;

namespace DemocracyBot.Data; 

public static class TruthOrDareService {

    private static string[] _truths = null!;
    private static string[] _dares = null!;

    public static string RandomTruth => _truths[Program.Random.Next(_truths.Length)];
    
    public static string RandomDare => _dares[Program.Random.Next(_dares.Length)];

    public static void Init() {
        _truths = File.ReadAllLines("Data/RawData/truth.txt");
        _dares = File.ReadAllLines("Data/RawData/dare.txt");
        Logger.Info($"Loaded {_truths.Length} truths and {_dares.Length} dares");
    }
    
}