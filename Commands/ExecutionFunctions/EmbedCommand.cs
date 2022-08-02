using Discord;
using Discord.WebSocket;

namespace DemocracyBot.Commands.ExecutionFunctions; 

public class EmbedCommand : ICommandExecutionHandler {
    public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {

        await cmd.DeferAsync();
        
        string title = cmd.GetArgument<string?>("title") ?? "";
        string body = cmd.GetArgument<string>("body")!;
        string colour = cmd.GetArgument<string?>("colour") ?? "Blue";
        string footer = cmd.GetArgument<string?>("footer") ?? "";
        string url = cmd.GetArgument<string?>("url") ?? "";
        string imageUrl = cmd.GetArgument<string?>("image-url") ?? "";
        string thumbnailUrl = cmd.GetArgument<string?>("thumbnail-url") ?? "";

        Color color;
        switch (colour.ToLower()) {
            case "blue":
                color = Color.Blue;
                break;
            case "green":
                color = Color.Green;
                break;
            case "red":
                color = Color.Red;
                break;
            case "gold":
            case "yellow":
                color = Color.Gold;
                break;
            case "purple":
                color = Color.Purple;
                break;
            case "orange":
                color = Color.Orange;
                break;
            case "magenta":
                color = Color.Magenta;
                break;
            case "dark blue":
            case "darkblue":
                color = Color.DarkBlue;
                break;
            case "gray":
            case "grey":
                color = Color.LightGrey;
                break;
            case "black":
                color = Color.DarkerGrey;
                break;
            default:
                await cmd.ModifyWithEmbedAsync("Usage", "Invalid colour", ResponseType.Error);
                return;
        }

        EmbedBuilder embed = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(body)
            .WithColor(color)
            .WithFooter(footer)
            .WithUrl(url)
            .WithImageUrl(imageUrl)
            .WithThumbnailUrl(thumbnailUrl);

        await cmd.Channel.SendMessageAsync(embed: embed.Build());
        await cmd.DeleteOriginalResponseAsync();
    }
}