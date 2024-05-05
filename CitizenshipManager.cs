using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;
using SimpleDiscordNet.Buttons;
using SimpleDiscordNet.Commands;

namespace DemocracyBot;

// ReSharper disable once ClassNeverInstantiated.Global
public class CitizenshipManager {
    public static DiscordSocketClient? Discord;

    public static async Task UserJoinedGuild(SocketGuildUser user) {
        if (user.IsBot) {
            return;
        }
        
        if (user.Guild.Id != Utils.Pucv("server_id")) {
            return;  // Wrong server
        }

        if (Program.StorageService.IsCitizen(user.Id)) {
            await GiveRole(user);
            return;
        }
        
        // Start a citizenship vote
        await TriggerCitizenshipApplication(user);
    }

    public static async Task TriggerCitizenshipApplication(IUser user) {
        ISocketMessageChannel channel = GetChannel();

        ButtonBuilder approveButton = new("Approve", "approve-citizen", ButtonStyle.Success,
            emote: Emoji.Parse(":white_check_mark:"));
        ButtonBuilder rejectButton =
            new("Reject", "reject-citizen", ButtonStyle.Danger, emote: Emoji.Parse(":x:"));

        MessageComponent component = new ComponentBuilder()
            .WithButton(approveButton)
            .WithButton(rejectButton)
            .Build();
        await channel.SendMessageAsync(embed: GetCitizenshipVoteEmbedFor(user), components: component);
    }

    private static async Task GiveRole(IGuildUser user) {
        await user.AddRoleAsync(Utils.Pucv("citizen_role"));
    }

    private static ISocketMessageChannel GetChannel() {
        return (Discord!.GetGuild(Utils.Pucv("server_id")).GetChannel(Utils.Pucv("citizenship_vote_channel")) as ISocketMessageChannel)!;
    }

    private static Embed GetCitizenshipVoteEmbedFor(IUser user) {
        bool isCitizen = user.IsCitizen();
        
        if (!isCitizen) {
            int votesFor = Program.StorageService.CountCitizenshipVotesFor(user.Id);
            int requiredVotes = Utils.GetCitizenshipVotesRequired();
            Embed voteMsg = CommandUtils.GetEmbed(
                "Citizenship Vote: " + user.Username,
                $"Should {user.Mention} be made a citizen?\n**Approvals:** *{votesFor}*/*{requiredVotes}*", 
                ResponseType.Info, 
                new EmbedFooterBuilder().WithText(user.Id.ToString()));
            return voteMsg;
        }
        else {
            Embed voteMsg = CommandUtils.GetEmbed(
                "Citizenship Vote: " + user.Username,
                $"**Approved!**", 
                ResponseType.Success, 
                new EmbedFooterBuilder().WithText(user.Id.ToString()));
            return voteMsg;
        }
    }

    private static async void GrantCitizenship(ulong userid, SocketMessageComponent interaction) {
        ISocketMessageChannel channel = GetChannel();
        SocketGuildUser? user = Discord!.GetGuild(Utils.Pucv("server_id")).GetUser(userid);

        Program.StorageService.AddCitizen(userid);
        // await interaction.Message.ModifyAsync(m => {
        //     m.Embed = GetCitizenshipVoteEmbedFor(user);
        //     m.Components = null;
        // });
        await interaction.Message.DeleteAsync();
        
        if (user == null) {  // Not much we can do
            return;
        }

        await channel.SendMessageAsync($"{user.Mention} is now a citizen!");
        await user.AddRoleAsync(Utils.Pucv("citizen_role"));
    }

    [ButtonListener("approve-citizen")]
    public async Task Approve(SocketMessageComponent component, DiscordSocketClient client) {
        if (!component.User.IsCitizen()) {
            await component.RespondWithEmbedAsync(
                "Citizenship",
                "Only existing citizens can vote to approve new citizens.", 
                ResponseType.Error, 
                ephemeral: true);
            return;
        }
        
        string targetIdString = component.Message.Embeds.First().Footer!.Value.Text;
        ulong targetId = ulong.Parse(targetIdString);
        IUser? user = await client.GetUserAsync(targetId);
        
        if (user == null) {
            await component.RespondWithEmbedAsync(
                "Citizenship",
                "This user no longer exists.", 
                ResponseType.Error, 
                ephemeral: true);
            return;
        }

        if (user.IsCitizen()) {
            await component.RespondWithEmbedAsync(
                "Citizenship", 
                $"{user.Mention} is already a citizen!",
                ephemeral: true);
            return;
        }
        
        Program.StorageService.RegisterCitizenshipVote(component.User.Id, targetId);
        await component.RespondWithEmbedAsync(
            "Citizenship", 
            $"You have voted to approve {user.Mention}'s application!",
            ephemeral: true);

        if (Program.StorageService.CountCitizenshipVotesFor(user.Id) >= Utils.GetCitizenshipVotesRequired()) {  // Approved
            GrantCitizenship(targetId, component);
            return;
        }

        await component.Message.ModifyAsync(p => p.Embed = GetCitizenshipVoteEmbedFor(user));
    }
    
    [ButtonListener("reject-citizen")]
    public async Task Reject(SocketMessageComponent component, DiscordSocketClient client) {
        if (!component.User.IsCitizen()) {
            await component.RespondWithEmbedAsync(
                "Citizenship",
                "Only existing citizens can vote to approve new citizens.", 
                ResponseType.Error, 
                ephemeral: true);
            return;
        }
        
        string targetIdString = component.Message.Embeds.First().Footer!.Value.Text;
        ulong targetId = ulong.Parse(targetIdString);
        IUser? user = await client.GetUserAsync(targetId);
        if (user == null) {
            await component.RespondWithEmbedAsync(
                "Citizenship",
                "This user no longer exists.", 
                ResponseType.Error, 
                ephemeral: true);
            return;
        }
        
        Program.StorageService.RevokeCitizenshipVote(component.User.Id, targetId);
        await component.RespondWithEmbedAsync(
            "Citizenship",
            $"You have withdrawn your approval of {user.Mention}'s application.", 
            ResponseType.Success, 
            ephemeral: true);
        
        await component.Message.ModifyAsync(p => p.Embed = GetCitizenshipVoteEmbedFor(user));
    }
    
}