using Discord.WebSocket;

namespace DemocracyBot.Commands; 

public interface ICommandExecutionHandler {
    public Task Execute(SocketSlashCommand cmd, DiscordSocketClient client);
}