using Discord;
using Discord.WebSocket;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class VoteCommand : ICommandExecutionHandler {
    public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        IUser voteUser = cmd.GetArgument<IUser>("president")!;
        
        
    }
}