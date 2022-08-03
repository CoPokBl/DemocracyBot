using DemocracyBot.Commands;
using DemocracyBot.Data.Schemas;
using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;

namespace DemocracyBot.Data; 

public class TimeCheckService {
    private static readonly TimeSpan TermLength = new(0, 0, 60);

    public static void StartThread(DiscordSocketClient client) {
        // Start the thread that checks for events
        new Thread(() => {
            while (true) {
                Update(client);
                Thread.Sleep(new TimeSpan(0, 0, 5)); // Run every 5 seconds
            }
        }).Start();
    }

    private static async void Update(DiscordSocketClient client) {
        
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
                }
                catch (Exception) {
                    // Ignore errors and have the winner be the bot
                    // The only error that should occur is if no one voted
                }

                // Add relative timestamp because it updates automatically on the client
                TimestampTag timestamp = TimestampTag.FromDateTime(
                    DateTime.Now.Add(TermLength), 
                    TimestampTagStyles.Relative);
                
                SocketUser winnerUser = client.GetUser(winner);
                await GetAnnouncementsChannel(client).SendMessageAsync(
                    embed: CommandManager.GetEmbed(
                        "The Poll Has Ended!", 
                        $"The Winner is: {winnerUser.Mention} with {votes} votes! (Next election: {timestamp})", 
                        ResponseType.Success
                    ).Build()
                );

                SocketGuild guild = client.GetGuild(ulong.Parse(Program.Config!["server_id"]));
                SocketGuildUser winnerMember = guild.GetUser(winner);
                
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
                    TermEnd = DateTime.UtcNow.Add(TermLength).ToBinary()
                });
                
                // Dont check the term
                return;
            }
        }

        // Check to see if the term is over
        Term? term = Program.StorageService.GetCurrentTerm();
        if (term != null && DateTime.UtcNow > DateTime.FromBinary(term.TermEnd)) TermEnd(client);
        
        // If there has never been an election and there isn't one currently then start one
        if (poll == null && term == null) TermEnd(client);
    }

    private static async void TermEnd(DiscordSocketClient client) {
        Program.StorageService.StartNewPoll();
            
        // Add relative timestamp because it updates automatically on the client
        TimestampTag timestamp = TimestampTag.FromDateTime(
            DateTime.FromBinary(Program.StorageService.GetCurrentPoll()!.PollEnd).ToLocalTime(), 
            TimestampTagStyles.Relative);
                
        await GetAnnouncementsChannel(client).SendMessageAsync(
            embed: CommandManager.GetEmbed(
                $"The Election Has Began! (Ends in {timestamp})", 
                "Do /vote to vote for the next president!", 
                ResponseType.Success
            ).Build()
        );
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