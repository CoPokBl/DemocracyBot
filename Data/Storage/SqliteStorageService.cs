using System.Data.SQLite;
using DemocracyBot.Data.Schemas;
using GeneralPurposeLib;

namespace DemocracyBot.Data.Storage;

public class SqliteStorageService : IStorageService {
    private const string ConnectionString = "Data Source=db.dat";
    private SQLiteConnection _connection = null!;
    
    public void Init() {
        _connection = new SQLiteConnection(ConnectionString);
        _connection.Open();
        CreateTables();

        if (GetCurrentTerm() != null) return; // We need to make a blank term which ends in vote time
        double hoursTillEnd = GlobalConfig.Config["poll_time"];
        DateTime start = DateTime.Now;
        DateTime end = start + TimeSpan.FromHours(hoursTillEnd);
        CreateTerm(0, start, end);
        
        // Print every citizen
        using SQLiteCommand cmd = new("SELECT user FROM citizen_status WHERE is_citizen = TRUE;", _connection);
        using SQLiteDataReader reader = cmd.ExecuteReader();
        while (reader.Read()) {
            Logger.Info(reader.GetInt64(0).ToString());
        }
    }

    private void CreateTables() {
        SQLiteCommand cmd = new(@"
CREATE TABLE IF NOT EXISTS votes (
    voter BIGINT UNSIGNED PRIMARY KEY, 
    votee BIGINT UNSIGNED
);

CREATE TABLE IF NOT EXISTS citizen_status (
    user BIGINT UNSIGNED PRIMARY KEY,
    is_citizen BOOLEAN
);

CREATE TABLE IF NOT EXISTS citizenship_votes (
    id INTEGER PRIMARY KEY autoincrement,
    voter BIGINT UNSIGNED,
    subject BIGINT UNSIGNED
);

CREATE TABLE IF NOT EXISTS terms (
    id INTEGER PRIMARY KEY autoincrement, 
    president BIGINT,
    term_start DATETIME,
    term_end DATETIME
);

CREATE TABLE IF NOT EXISTS riot_votes (
    voter BIGINT UNSIGNED PRIMARY KEY
);

CREATE TABLE IF NOT EXISTS state (
    val VARCHAR(64) PRIMARY KEY
);
", _connection);
        cmd.ExecuteNonQuery();
    }
    
    public void Deinit() {
        _connection.Dispose();
    }

    public void RegisterVote(ulong user, ulong vote) {
        using SQLiteCommand cmd = new("INSERT OR IGNORE INTO votes (voter, votee) VALUES (@voter, @votee);", _connection);
        cmd.Parameters.AddWithValue("voter", user);
        cmd.Parameters.AddWithValue("votee", vote);
        cmd.ExecuteNonQuery();
    }

    public void RegisterCitizenshipVote(ulong user, ulong target) {
        using SQLiteCommand cmd = new("INSERT OR REPLACE INTO citizenship_votes (voter, subject) VALUES (@voter, @subject);", _connection);
        cmd.Parameters.AddWithValue("voter", user);
        cmd.Parameters.AddWithValue("subject", target);
        cmd.ExecuteNonQuery();
    }

    public void RevokeCitizenshipVote(ulong user, ulong target) {
        using SQLiteCommand cmd = new("DELETE FROM citizenship_votes WHERE voter = @voter AND subject = @subject;", _connection);
        cmd.Parameters.AddWithValue("voter", user);
        cmd.Parameters.AddWithValue("subject", target);
        cmd.ExecuteNonQuery();
    }

    public void AddCitizen(ulong user) {
        using SQLiteCommand cmd = new("INSERT INTO citizen_status (user, is_citizen) VALUES (@user, TRUE);", _connection);
        cmd.Parameters.AddWithValue("user", user);
        cmd.ExecuteNonQuery();
    }

    public void CreateTerm(ulong president, DateTime start, DateTime end) {
        using SQLiteCommand cmd = new("INSERT INTO terms (president, term_start, term_end) VALUES (@president, @start, @end);", _connection);
        cmd.Parameters.AddWithValue("president", president);
        cmd.Parameters.AddWithValue("start", start);
        cmd.Parameters.AddWithValue("end", end);
        cmd.ExecuteNonQuery();
    }

    public void RegisterRiot(ulong user) {
        using SQLiteCommand cmd = new("INSERT INTO riot_votes (voter) VALUES (@user);", _connection);
        cmd.Parameters.AddWithValue("user", user);
        cmd.ExecuteNonQuery();
    }

    public void RevokeRiot(ulong user) {
        using SQLiteCommand cmd = new("DELETE FROM riot_votes WHERE voter = @user;", _connection);
        cmd.Parameters.AddWithValue("user", user);
        cmd.ExecuteNonQuery();
    }

    public void SetCurrentTermEnd(DateTime end) {
        using SQLiteCommand cmd = new("UPDATE terms SET term_end = @end WHERE id = (SELECT MAX(id) FROM terms);", _connection);
        cmd.Parameters.AddWithValue("end", end);
        cmd.ExecuteNonQuery();
    }

    public void SetState(string state) {  // Clear table and add state
        using SQLiteCommand cmd = new("DELETE FROM state;", _connection);
        cmd.ExecuteNonQuery();
        cmd.CommandText = "INSERT INTO state (val) VALUES (@state);";
        cmd.Parameters.AddWithValue("state", state);
        cmd.ExecuteNonQuery();
    }

    public Dictionary<ulong, int> TallyVotes() {
        using SQLiteCommand cmd = new("SELECT votee, COUNT(*) FROM votes GROUP BY votee;", _connection);
        using SQLiteDataReader reader = cmd.ExecuteReader();
        Dictionary<ulong, int> votes = new();
        while (reader.Read()) {
            votes.Add((ulong)(long)reader[0], reader.GetInt32(1));
        }
        return votes;
    }

    public int CountCitizenshipVotesFor(ulong user) {
        using SQLiteCommand cmd = new("SELECT COUNT(*) FROM citizenship_votes WHERE subject = @user;", _connection);
        cmd.Parameters.AddWithValue("user", user);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int CountCitizens() {
        using SQLiteCommand cmd = new("SELECT COUNT(*) FROM citizen_status WHERE is_citizen = TRUE;", _connection);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool IsCitizen(ulong user) {
        using SQLiteCommand cmd = new("SELECT COUNT(*) FROM citizen_status WHERE user = @user AND is_citizen = TRUE;", _connection);
        cmd.Parameters.AddWithValue("user", user);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public int CountRioters() {
        using SQLiteCommand cmd = new("SELECT COUNT(*) FROM riot_votes;", _connection);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public Term? GetCurrentTerm() {
        using SQLiteCommand cmd = new("SELECT * FROM terms ORDER BY id DESC LIMIT 1;", _connection);
        using SQLiteDataReader reader = cmd.ExecuteReader();
        if (!reader.Read()) {
            return null;
        }
        return new Term {
            Id = reader.GetInt32(0),
            PresidentId = (ulong)reader.GetInt64(1),
            TermStart = reader.GetDateTime(2),
            TermEnd = reader.GetDateTime(3)
        };
    }

    public bool IsRioting(ulong user) {
        using SQLiteCommand cmd = new("SELECT COUNT(*) FROM riot_votes WHERE voter = @user;", _connection);
        cmd.Parameters.AddWithValue("user", user);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public string GetState() {
        using SQLiteCommand cmd = new("SELECT val FROM state;", _connection);
        return cmd.ExecuteScalar()?.ToString() ?? "none";
    }

    public void ClearVotes() {
        using SQLiteCommand cmd = new("DELETE FROM votes;", _connection);
        cmd.ExecuteNonQuery();
    }

    public void ClearRioters() {
        using SQLiteCommand cmd = new("DELETE FROM riot_votes;", _connection);
        cmd.ExecuteNonQuery();
    }
}