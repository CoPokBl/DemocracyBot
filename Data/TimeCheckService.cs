using System.Diagnostics;
using DemocracyBot.Commands;
using DemocracyBot.Data.Schemas;
using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;

namespace DemocracyBot.Data; 

public static class TimeCheckService {
    private static TimeSpan _termLength;
    private static DateTime _nextSave = DateTime.Now.AddMinutes(1);
    private static int _guildMembersMinusBots;
    private static int _countCount = 99;
    private static DiscordSocketClient? _client;
    
    private static int _activeThreadCount;
    private static int _activeUpdatesCount;

    private static int ActiveThreads {
        get => _activeThreadCount;
        set {
            if (value is > 1 or < 0) {
                Logger.Error("App tried to set active threads to " + value);
                string stackTrace = new StackTrace().ToString();
                GetAnnouncementsChannel(_client!).SendMessageAsync("App tried to set active threads to " + value + "\n" + stackTrace);
                throw new Exception("App tried to set active threads to " + value);
            }
            _activeThreadCount = value;
        }
    }
    
    private static int ActiveUpdates {
        get => _activeUpdatesCount;
        set {
            if (value is > 1 or < 0) {
                Logger.Error("App tried to set active updates to " + value);
                string stackTrace = new StackTrace().ToString();
                GetAnnouncementsChannel(_client!).SendMessageAsync("App tried to set active threads to " + value + "\n" + stackTrace);
                throw new Exception("App tried to set active updates to " + value);
            }
            _activeUpdatesCount = value;
        }
    }

    public static void StartThread(DiscordSocketClient client) {
        _client = client;
        _termLength = TimeSpan.FromHours(Convert.ToDouble(Program.Config!["term_length"]));

        // Start the thread that checks for events
        new Thread(() => {
            ActiveThreads++;
            while (true) {
                try {
                    Update(client);
                }
                catch (Exception e) {
                    Logger.Error("Error in TimeCheckService.Update: " + e.Message);
                    Logger.Error(e);
                    GetAnnouncementsChannel(client).SendMessageAsync(e.ToString());
                }
                Thread.Sleep(new TimeSpan(0, 0, 5)); // Run every 5 seconds
            }
        }).Start();
    }

    private static async void Update(DiscordSocketClient client) {
        ActiveUpdates++;
        SocketGuild guild = client.GetGuild(ulong.Parse(Program.Config!["server_id"]));

        _countCount++;
        if (_countCount >= 100) {
            _countCount = 0;
            _guildMembersMinusBots = guild.Users.Count(u => !u.IsBot);
            //Logger.Debug("Guild members minus bots update: " + _guildMembersMinusBots);
        }
        
        // Check for save event
        if (_nextSave < DateTime.Now) {
            Program.StorageService.Save();
            _nextSave = DateTime.Now.AddMinutes(1);
        }
        
        // Check to see if the poll is over
        Poll? poll = Program.StorageService.GetCurrentPoll();
        if (poll != null) {
            DateTime time = DateTime.FromBinary(poll.PollEnd);
            
            // if its past PollEnd, then end the poll
            if (DateTime.UtcNow > time) {
                ulong winner = client.CurrentUser.Id;
                int votes = 0;
                try {
                    Program.StorageService.EndPoll(out winner, out votes);
                    Logger.Debug("Poll Was Ended Successfully, winner: " + winner + " votes: " + votes);
                }
                catch (Exception e) {
                    Logger.Debug(e);
                    // Ignore errors and have the winner be the bot
                    // The only error that should occur is if no one voted
                }
                Program.StorageService.NullifyPoll();

                // Add relative timestamp because it updates automatically on the client
                TimestampTag timestamp = TimestampTag.FromDateTime(
                    DateTime.Now.Add(_termLength), 
                    TimestampTagStyles.Relative);
                
                SocketGuildUser winnerMember = guild.GetUser(winner);

                if (winnerMember == null) {
                    // That user is no longer in the server
                    Debug.Assert(client != null);
                    SocketUser user = client.GetUser(winner);
                    string winnerUserMention;
                    if (user == null) {
                        winnerUserMention = "<@" + winner + ">";
                    }
                    else {
                        winnerUserMention = user.Mention;
                    }
                    Debug.Assert(timestamp != null);
                    Debug.Assert(winnerMember == null);
                    await GetAnnouncementsChannel(client).SendMessageAsync(
                        embed: CommandManager.GetEmbed(
                            "The Poll Has Ended!",
                            $"The winner is: {winnerUserMention} with {votes} votes! " +
                            "However they are nowhere to be found, so I'll be stepping in as acting president. " +
                            $"(Next election: {timestamp})",
                            ResponseType.Success
                        ).Build()
                    );
                    winnerMember = guild.GetUser(client.CurrentUser.Id);
                }
                else {
                    await GetAnnouncementsChannel(client).SendMessageAsync(
                        embed: CommandManager.GetEmbed(
                            "The Poll Has Ended!", 
                            $"The Winner is: {winnerMember.Mention} with {votes} votes! (Next election: {timestamp})", 
                            ResponseType.Success
                        ).Build()
                    );
                }
                Logger.Debug("Poll Ended Message Sent");

                // Take role away from current president
                // Make sure this is before giving the role because if the same person was elected again then it would give the role then take it
                Term? cTerm = Program.StorageService.GetCurrentTerm();
                if (cTerm != null) {
                    SocketGuildUser oldPresident = guild.GetUser(cTerm.PresidentId);
                    oldPresident.RemoveRoleAsync(ulong.Parse(Program.Config["president_role_id"])).Wait();
                }

                // Give them the role
                await winnerMember.AddRoleAsync(ulong.Parse(Program.Config["president_role_id"]));
                
                // Set new president
                Program.StorageService.SetCurrentTerm(new Term {
                    PresidentId = winner,
                    TermStart = DateTime.UtcNow.ToBinary(),
                    TermEnd = DateTime.UtcNow.Add(_termLength).ToBinary(),
                    RiotVotes = new List<ulong>()
                });
                
                // Dont check the term
                ActiveUpdates--;
                return;
            }
        }

        // Check to see if the term is over
        Term? term = Program.StorageService.GetCurrentTerm();
        if (term != null && poll == null && DateTime.UtcNow > DateTime.FromBinary(term.TermEnd)) TermEnd(client);
        
        // Check to see if a riot has been triggered
        if (term != null) {
            double percentOfPeopleWantingToOverthrow = term.RiotVotes.Count / ((double)_guildMembersMinusBots - 1);
            if (percentOfPeopleWantingToOverthrow > 0.5) {
                // Riot has been triggered
                Logger.Debug("Riot has been triggered");
                // Get the president
                SocketGuildUser president = guild.GetUser(term.PresidentId);
                await president.RemoveRoleAsync(ulong.Parse(Program.Config["president_role_id"]));
                await GetAnnouncementsChannel(client)
                    .SendMessageAsync(embed: 
                        CommandManager.GetEmbed(
                            "Riot!", 
                            "The president has been overthrown!", 
                            ResponseType.Success).Build());
                Logger.Debug("Riot Message Sent");
                term = null;
                Program.StorageService.SetCurrentTerm(term!);
            }
        }

        // If there has never been an election and there isn't one currently then start one
        if (poll == null && term == null) TermEnd(client);
        ActiveUpdates--;
    }

    private static async void TermEnd(DiscordSocketClient client) {
        Logger.Debug("Term Ended");
        Program.StorageService.StartNewPoll();
        
        // Add relative timestamp because it updates automatically on the client
        TimestampTag timestamp = TimestampTag.FromDateTime(
            DateTime.FromBinary(Program.StorageService.GetCurrentPoll()!.PollEnd).ToLocalTime(), 
            TimestampTagStyles.Relative);
                
        await GetAnnouncementsChannel(client).SendMessageAsync(
            embed: CommandManager.GetEmbed(
                $"The Election Has Began! (Ends {timestamp})", 
                "Do /vote to vote for the next president!", 
                ResponseType.Success
            ).Build()
        );
        Logger.Debug("Election Message Sent");
    }

    private static SocketTextChannel GetAnnouncementsChannel(DiscordSocketClient client) {
        if (client == null) throw new Exception("client is null");
        Logger.Debug(Program.Config!["server_id"]);
        SocketGuild? guild = client.GetGuild(ulong.Parse(Program.Config["server_id"]));
        if (guild == null) {
            Logger.Debug("guild is null");
        }
        SocketGuildChannel? server = guild!.GetChannel(ulong.Parse(Program.Config["announcements_channel_id"]));
        if (server == null) {
            Logger.Debug("server is null");
        }
        return (SocketTextChannel) server!;
    }
    
}