using System.Text.Json;
using Discord.WebSocket;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class ImageSearchCommand : ICommandExecutionHandler {
    public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        string query = cmd.GetArgument<string>("query")!;

        await cmd.DeferAsync();

        try {
            HttpClient http = new HttpClient();
            HttpResponseMessage response = await http.PostAsync("https://search.serble.net/", new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("category_images", "1"),
                new KeyValuePair<string, string>("q", query),
                new KeyValuePair<string, string>("format", "json")
            }));
            
            JsonDocument content = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            JsonElement results = content.RootElement.GetProperty("results");
            string url = results[Program.Random.Next(results.GetArrayLength())].GetProperty("img_src").GetString()!;
            await cmd.ModifyBodyTextAsync(url);
        }
        catch (Exception) {
            await cmd.ModifyWithEmbedAsync("Error", "Error: Could not find image.", ResponseType.Error);
        }
    }
}