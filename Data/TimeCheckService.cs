using DemocracyBot.Commands;
using DemocracyBot.Data.Schemas;
using Discord.WebSocket;
using GeneralPurposeLib;

namespace DemocracyBot.Data; 

public class TimeCheckService {

    public static void StartThread(DiscordSocketClient client) {
        new Thread(() => {
            while (true) {
                Update(client);
                Thread.Sleep(60000);
            }
        }).Start();
    }

    public static void Update(DiscordSocketClient client) {
        Poll? poll = Program.StorageService.GetCurrentPoll();

        if (poll != null) {
            DateTime time = DateTime.FromBinary(poll.PollEnd);
            
            // if its past PollEnd, then end the poll
            if (DateTime.UtcNow > time) {
                Program.StorageService.EndPoll(out ulong winner, out int votes);

                GetAnnouncementsChannel(client).SendMessageAsync(
                    embed: CommandManager.GetEmbed(
                        "The Poll Has Ended!", 
                        $"The Winner is: {client.GetUser(winner)} with {votes} votes!", 
                        ResponseType.Success
                    ).Build()
                );
                
                // Set new president
                Program.StorageService.SetCurrentTerm(new Term {
                    PresidentId = winner,
                    TermStart = DateTime.UtcNow.ToBinary(),
                    TermEnd = DateTime.UtcNow.AddDays(1).ToBinary()
                });
                
                // Dont check the term
                return;
            }
        }

        Term? term = Program.StorageService.GetCurrentTerm();

        if (term != null) {
            DateTime time = DateTime.FromBinary(term.TermEnd);
            
            // if its past TermEnd, then start the new poll
            if (DateTime.UtcNow > time) {
                Program.StorageService.StartNewPoll();
                
                GetAnnouncementsChannel(client).SendMessageAsync(
                    embed: CommandManager.GetEmbed(
                        "The New Poll has Started!", 
                        $"Do /vote to vote for the next president!", 
                        ResponseType.Success
                    ).Build()
                );
            }
        }
        
        if (poll == null && term == null) {
            Program.StorageService.StartNewPoll();
            
            GetAnnouncementsChannel(client).SendMessageAsync(
                embed: CommandManager.GetEmbed(
                    "The New Poll has Started!", 
                    $"Do /vote to vote for the next president!", 
                    ResponseType.Success
                ).Build()
            );
        }
    }

    private static SocketTextChannel GetAnnouncementsChannel(DiscordSocketClient client) {
        if (client == null) throw new Exception("client is null");
        Logger.Debug(Program.Config!["server_id"]);
        SocketGuild? guild = client.GetGuild(ulong.Parse(Program.Config["server_id"]));
        if (guild == null) {
            Logger.Debug("guild is null");
        }
        SocketGuildChannel? server = guild.GetChannel(ulong.Parse(Program.Config["announcements_channel_id"]));
        if (server == null) {
            Logger.Debug("server is null");
        }
        return (SocketTextChannel) server;
    }
    
}