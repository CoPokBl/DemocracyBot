using System.Diagnostics;
using DemocracyBot.Data;
using DemocracyBot.Data.Schemas;
using DemocracyBot.Data.Storage;
using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;
using SimpleDiscordNet.Commands;

namespace DemocracyBot;

public static class Utils {

    public static bool IsPollRunning(this IStorageService storage) {
        Term? term = storage.GetCurrentTerm();
        Debug.Assert(term != null);
        
        // Return whether we are within poll length hours of the end of the current term
        double pollLength = GlobalConfig.Config["poll_time"];
        return term.TermEnd - DateTime.Now < TimeSpan.FromHours(pollLength);
    }

    public static Poll? GetPoll(this IStorageService storage) {
        Poll poll = new();

        if (!IsPollRunning(storage)) {
            return null;
        }
        
        Term? term = storage.GetCurrentTerm();
        Debug.Assert(term != null);
        
        poll.PollEnd = term.TermEnd;
        poll.PollStart = term.TermEnd - TimeSpan.FromHours(GlobalConfig.Config["poll_time"]);
        poll.Votes = storage.TallyVotes();
        return poll;
    }

    public static int GetRequiredRioters(this IStorageService storage) {
        return (int) Math.Ceiling(storage.CountCitizens() / 2.0);
    }

    public static async void TriggerElection(DiscordSocketClient discord) {
        DateTime end = DateTime.Now + TimeSpan.FromHours(GlobalConfig.Config["poll_time"]);
        Program.StorageService.SetCurrentTermEnd(end);

        TimeCheckService.UpdateTimes();
    }

    public static async void Announce(DiscordSocketClient discord, string msg, string? title = null) {
        await GetAnnouncementsChannel(discord)
            .SendMessageAsync(embed: 
                CommandUtils.GetEmbed(
                    title ?? "Announcement", 
                    msg, 
                    ResponseType.Info));
    }
    
    private static SocketTextChannel GetAnnouncementsChannel(DiscordSocketClient client) {
        if (client == null) throw new Exception("client is null");
        Logger.Debug(GlobalConfig.Config["server_id"].Text);
        SocketGuild? guild = client.GetGuild(ulong.Parse(GlobalConfig.Config["server_id"]));
        if (guild == null) {
            Logger.Debug("guild is null");
        }
        SocketGuildChannel? server = guild!.GetChannel(ulong.Parse(GlobalConfig.Config["announcements_channel_id"]));
        if (server == null) {
            Logger.Debug("server is null");
        }
        return (SocketTextChannel) server!;
    }
    
    /// <summary>
    /// Parse ulong config value
    /// </summary>
    /// <param name="s">The config key</param>
    /// <returns>The ulong value</returns>
    public static ulong Pucv(string s) {
        return ulong.Parse(GlobalConfig.Config[s].Text);
    }

    public static bool IsCitizen(this IUser user) {
        return Program.StorageService.IsCitizen(user.Id);
    }

    /// <summary>
    /// The amount of votes required is ceil of two thirds of the citizen count
    /// </summary>
    /// <returns></returns>
    public static int GetCitizenshipVotesRequired() {
        return (int) Math.Ceiling(Program.StorageService.CountCitizens() * 2.0 / 3.0);
    }

    public static bool IsPresident(this IUser user) {
        return Program.StorageService.GetCurrentTerm()!.PresidentId == user.Id;
    }
    
}