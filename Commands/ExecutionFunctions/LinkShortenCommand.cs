using Discord.WebSocket;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class LinkShortenCommand : ICommandExecutionHandler {
    public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        // Get args
        string link = cmd.GetArgument<string>("link")!;
        string name = cmd.GetArgument<string>("name") ?? RandomString(10);

        await cmd.DeferAsync();

        // Send a http request to link.serble.net
        string content;
        try {
            HttpClient http = new ();
            HttpResponseMessage response = await http.PostAsync("https://link.serble.net", new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("location", link),
                new KeyValuePair<string, string>("name", name)
            }));
            content = await response.Content.ReadAsStringAsync();
        }
        catch (Exception) {
            await cmd.ModifyWithEmbedAsync("Error", "Failed to shorten link", ResponseType.Error);
            return;
        }

        // Send the response to the discord channel
        await cmd.ModifyBodyTextAsync(content);
    }
    
    // Function to generate a random string of length n
    private static string RandomString(int length) {
        Random random = new();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
}