using DemocracyBot.Data;
using Discord.WebSocket;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class DareCommand : ICommandExecutionHandler {
    public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        await cmd.RespondWithEmbedAsync("Truth", TruthOrDareService.RandomDare);
    }
}