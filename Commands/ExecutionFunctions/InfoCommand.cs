using Discord.WebSocket;

namespace SerbleBot.Commands.ExecutionFunctions; 

public class InfoCommand : ICommandExecutionHandler {
    
    public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        await cmd.RespondAsync($"I am SerbleBot {Program.Version}");
    }
    
}