using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;
using SimpleDiscordNet.Commands;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class VoteCommand {
    
    [SlashCommand("vote", "Cast your vote for the next president")]
    [SlashCommandArgument("president", "The person to vote for president", true, ApplicationCommandOptionType.User)]
    [SlashCommandArgument("anonymous", "Whether or not to hide your vote", false, ApplicationCommandOptionType.Boolean)]
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        if (!Program.StorageService.IsPollRunning()) {
            await cmd.RespondWithEmbedAsync("Election", "There is no current poll running", ResponseType.Error);
            return;
        }
        
        if (!Program.StorageService.IsCitizen(cmd.User.Id)) {
            await cmd.RespondWithEmbedAsync("Election", "Only citizens can vote in elections.", ResponseType.Error);
            return;
        }
        
        IUser voteUser = cmd.GetArgument<IUser>("president")!;
        bool? anon = cmd.GetArgument<bool>("anonymous");
        if (!anon.HasValue) {
            anon = false;
        }
        SocketGuild guild = client.GetGuild(ulong.Parse(GlobalConfig.Config["server_id"]));
        SocketGuildUser voteMember = guild.GetUser(voteUser.Id);

        if (voteMember == null) {
            // User is not in the server
            await cmd.RespondWithEmbedAsync("Election", "You can only vote for members of the server.", ResponseType.Error);
            return;
        }

        if (!voteMember.IsCitizen()) {
            await cmd.RespondWithEmbedAsync("Election", "You can only vote for citizens.", ResponseType.Error);
            return;
        }

        Program.StorageService.RegisterVote(cmd.User.Id, voteUser.Id);
        await cmd.RespondWithEmbedAsync(
            "Election",
            $"You have voted for {voteUser.Username}",
            ResponseType.Success,
            footer: new EmbedFooterBuilder().WithText("You can change your vote at any time!"),
            ephemeral: anon.Value);
    }
}