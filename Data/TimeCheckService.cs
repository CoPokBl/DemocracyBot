using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;

namespace DemocracyBot.Data; 

public static class TimeCheckService {
    private static TimeSpan _termLength;
    private static DiscordSocketClient? _discord;
    private static Timer? _eventTimer;
    private static WaitingEvent? _upcomingEvent;

    private static WaitingEvent UpcomingEvent {
        get {
            if (_upcomingEvent == null) {
                LoadEvent();
                return _upcomingEvent!.Value;
            }
            return _upcomingEvent.Value;
        }

        set {
            _upcomingEvent = value;
            SetEvent();
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
        if (UpcomingEvent == WaitingEvent.None) {  // Check if it's already passed
            Logger.Info("State was none, if this isn't a new db then this is an error");
            UpcomingEvent = WaitingEvent.PollStart;
            CurrentEventComplete(UpcomingEvent);
        }

        switch (UpcomingEvent) {
            case WaitingEvent.TermEnd: {
                TimeSpan? timeTillEnd = null;
                if (timeTillEnd == null || timeTillEnd < TimeSpan.Zero) {
                    if (timeTillEnd != null) CurrentEventComplete(UpcomingEvent);
                    timeTillEnd = Program.StorageService.GetCurrentTerm()!.TermEnd - DateTime.Now;
                }
                EventTimer = new Timer(CurrentEventComplete, UpcomingEvent, timeTillEnd.Value, Timeout.InfiniteTimeSpan);
                break;
            }

            case WaitingEvent.PollStart: {
                TimeSpan? timeTillEnd = null;
                while (timeTillEnd == null || timeTillEnd < TimeSpan.Zero) {
                    if (timeTillEnd != null) CurrentEventComplete(UpcomingEvent);
                    timeTillEnd = Program.StorageService.GetCurrentTerm()!.TermEnd - TimeSpan.FromHours(GlobalConfig.Config["poll_time"]) - DateTime.Now;
                }
                EventTimer = new Timer(CurrentEventComplete, UpcomingEvent, timeTillEnd.Value, Timeout.InfiniteTimeSpan);
                break;
            }
            
            case WaitingEvent.None:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static async void CurrentEventComplete(object? o) {
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

                ulong newPresident = Program.StorageService.GetPoll()!.GetWinner();
                SocketGuildUser? gNewUser = guild.GetUser(newPresident);
                if (newPresident != 0 && gNewUser != null) {  // Can be zero in edge cases
                    await gNewUser.AddRoleAsync(roleId);
                }

                string name;
                if (gNewUser == null) {
                    SocketUser? newUser = _discord.GetUser(newPresident);
                    name = newUser == null ? newPresident.ToString() : newUser.Mention;
                }
                else {
                    name = gNewUser.Mention;
                }

                DateTime nextTermEnd = DateTime.Now + _termLength;
                string nextElection =
                    TimestampTag.FromDateTime(nextTermEnd - TimeSpan.FromHours(GlobalConfig.Config["poll_time"])).ToString(TimestampTagStyles.Relative);
                
                Program.StorageService.CreateTerm(newPresident, DateTime.Now, nextTermEnd);
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