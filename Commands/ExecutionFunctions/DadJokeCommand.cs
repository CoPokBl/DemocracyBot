using Discord.WebSocket;
using GeneralPurposeLib;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class DadJokeCommand : ICommandExecutionHandler {
    public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        await cmd.DeferAsync();
        
        try {
            HttpClient http = new ();
            http.DefaultRequestHeaders.Add("Accept", "text/plain");
            HttpResponseMessage response = await http.GetAsync("https://icanhazdadjoke.com/");

            string joke = await response.Content.ReadAsStringAsync();
            await cmd.ModifyBodyTextAsync(joke);
        }
        catch (Exception) {
            Logger.Warn("Dad Joke request failed");
            await cmd.ModifyWithEmbedAsync("Error", "Could not think of a dad joke.");
        }
    }
}