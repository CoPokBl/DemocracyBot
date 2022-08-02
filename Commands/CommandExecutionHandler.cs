using Discord.WebSocket;

namespace DemocracyBot.Commands; 

public interface ICommandExecutionHandler {
    public void Execute(SocketSlashCommand cmd, DiscordSocketClient client);
}