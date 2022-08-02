using System.Security.Cryptography;
using System.Text;
using Discord.WebSocket;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class Sha256Command : ICommandExecutionHandler {
    public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        string text = cmd.GetArgument<string>("text")!;
        
        // hash the text
        byte[] hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(text));
        string hashStr = BitConverter.ToString(hash).Replace("-", "").ToLower();
        await cmd.RespondWithEmbedAsync("SHA256", hashStr);
    }
}