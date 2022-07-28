using Discord.WebSocket;

namespace SerbleBot.Commands; 

public interface ICommandExecutionHandler {
    public void Execute(SocketSlashCommand cmd, DiscordSocketClient client);
}