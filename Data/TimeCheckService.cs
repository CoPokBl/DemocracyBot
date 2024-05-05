using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;

namespace DemocracyBot.Data; 

public static class TimeCheckService {
    private static TimeSpan _termLength;
    private static DiscordSocketClient? _discord;
    private static Timer? _eventTimer;
    private static WaitingEvent? _upcomingEvent;
    private static readonly object UpcomingEventLock = new();

    private static WaitingEvent UpcomingEvent {
        get {
            lock (UpcomingEventLock) {
                if (_upcomingEvent == null) {
                    LoadEvent();
                    return _upcomingEvent!.Value;
                }
                return _upcomingEvent.Value;
            }
        }

        set {
            lock (UpcomingEventLock) {
                _upcomingEvent = value;
                SetEvent();
            }
        }
    }

    private static Timer? EventTimer {
        get => _eventTimer;
        set {
            _eventTimer?.Dispose();
            _eventTimer = value;
        }
    }

    public static void Start(DiscordSocketClient client) {
        _discord = client;
        _termLength = TimeSpan.FromHours(GlobalConfig.Config["term_length"]);

        UpdateTimes();
    }

    public static void UpdateTimes() {
        _eventTimer?.Dispose();  // Make sure it doesn't continue to run

        if (UpcomingEvent == WaitingEvent.None) {  // Check if it's already passed
            Logger.Info("State was none, if this isn't a new db then this is an error");
            UpcomingEvent = WaitingEvent.PollStart;
            CurrentEventComplete(UpcomingEvent);
            return;
        }
        
        TimeSpan timeTillEnd = UpcomingEvent switch {
            WaitingEvent.TermEnd => Program.StorageService.GetCurrentTerm()!.TermEnd - DateTime.Now,
            WaitingEvent.PollStart => Program.StorageService.GetCurrentTerm()!.TermEnd - TimeSpan.FromHours(GlobalConfig.Config["poll_time"]) - DateTime.Now,
            WaitingEvent.None => throw new Exception("Invalid state"),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        if (timeTillEnd < TimeSpan.Zero) {
            CurrentEventComplete(UpcomingEvent);
            return;
        }
        
        EventTimer = new Timer(CurrentEventComplete, UpcomingEvent, timeTillEnd, Timeout.InfiniteTimeSpan);
    }
    
    private static async void CurrentEventComplete(object? o) {  // Calls UpdateTimes at the end
        WaitingEvent e = (WaitingEvent) o!;

        switch (e) {
            case WaitingEvent.TermEnd: {  // The election is over
                // Does president have the role?
                ulong oldPresident = Program.StorageService.GetCurrentTerm()!.PresidentId;
                SocketGuild guild = _discord!.GetGuild(Utils.Pucv("server_id"));
                SocketGuildUser? gOldUser = guild.GetUser(oldPresident);
                
                ulong roleId = Utils.Pucv("president_role_id");
                if (gOldUser != null && gOldUser.Roles.Any(r => r.Id == roleId)) {
                    await gOldUser.RemoveRoleAsync(roleId);
                }

                await guild.DownloadUsersAsync();  // We need this to actually get the user for some stupid reason
                ulong newPresident = Program.StorageService.GetPoll()!.GetWinner();
                SocketGuildUser? gNewUser = guild.GetUser(newPresident);
                
                if (newPresident != 0 && gNewUser != null) {  // Can be zero in edge cases
                    await gNewUser.AddRoleAsync(roleId);
                }

                string name;
                if (gNewUser == null) {
                    Logger.Warn($"User doesn't exist when trying to announce president: {newPresident}");
                    SocketUser? newUser = _discord.GetUser(newPresident);
                    name = newUser == null ? newPresident.ToString() : newUser.Mention;
                }
                else {
                    name = gNewUser.Mention;
                }

                DateTime nextTermEnd = DateTime.Now + _termLength;
                string nextElection =
                    TimestampTag.FromDateTime(nextTermEnd - TimeSpan.FromHours(GlobalConfig.Config["poll_time"])).ToString(TimestampTagStyles.Relative);

                // Reset the term and all relevant data points
                Program.StorageService.CreateTerm(newPresident, DateTime.Now, nextTermEnd);
                Program.StorageService.ClearVotes();
                Program.StorageService.ClearRioters();
                
                Utils.Announce(_discord, $"{name} has won the election! The next election is {nextElection}.");
                UpcomingEvent = WaitingEvent.PollStart;
                break;
            }

            case WaitingEvent.PollStart: {
                DateTime termEnd = Program.StorageService.GetCurrentTerm()!.TermEnd;
                string timestamp = TimestampTag.FromDateTime(termEnd).ToString(TimestampTagStyles.Relative);
                Utils.Announce(_discord!, $"The election has begun! Ends {timestamp}.");
                UpcomingEvent = WaitingEvent.TermEnd;
                break;
            }
            
            case WaitingEvent.None:
            default:
                throw new ArgumentOutOfRangeException();
        }

        UpdateTimes();
    }

    private static void LoadEvent() {
        string state = Program.StorageService.GetState();
        _upcomingEvent = state switch {
            "none" => WaitingEvent.None,
            "termend" => WaitingEvent.TermEnd,
            "pollstart" => WaitingEvent.PollStart,
            _ => throw new Exception("Invalid state")
        };
    }

    private static void SetEvent() {
        Program.StorageService.SetState(UpcomingEvent switch {
            WaitingEvent.None => "none",
            WaitingEvent.TermEnd => "termend",
            WaitingEvent.PollStart => "pollstart",
            _ => throw new Exception("Invalid state")
        });
    }

    

    enum WaitingEvent {
        TermEnd,
        PollStart,
        None
    }

}