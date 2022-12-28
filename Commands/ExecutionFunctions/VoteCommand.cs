using DemocracyBot.Data.Schemas;
using Discord;
using Discord.WebSocket;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class VoteCommand : ICommandExecutionHandler {
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        Poll? poll = Program.StorageService.GetCurrentPoll();

        if (poll == null) {
            await cmd.RespondWithEmbedAsync("Error", "There is no current poll running", ResponseType.Error);
            return;
        }
        
        IUser voteUser = cmd.GetArgument<IUser>("president")!;
        SocketGuild guild = client.GetGuild(ulong.Parse(Program.Config!["server_id"]));
        SocketGuildUser voteMember = guild.GetUser(voteUser.Id);

        if (voteMember == null) {
            // User is not in the server
            await cmd.RespondWithEmbedAsync("Error", "You can only vote for members of the server.", ResponseType.Error);
        }

        if (poll.Votes.ContainsKey(cmd.User.Id)) {
            poll.Votes[cmd.User.Id] = voteUser.Id;
        }
        else {
            poll.Votes.Add(cmd.User.Id, voteUser.Id);
        }

        EmbedBuilder embed = CommandManager.GetEmbed("Vote Successful", 
            $"You have voted for {voteUser.Username}", ResponseType.Success);
        
        embed.WithFooter(new EmbedFooterBuilder().WithText($"You can change your vote at any time!"));

        await cmd.RespondAsync(embed: embed.Build());
    }
}