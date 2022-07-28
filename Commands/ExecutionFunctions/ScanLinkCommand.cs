using Discord.WebSocket;
using GeneralPurposeLib;
using SerbleBot.Data;

namespace SerbleBot.Commands.ExecutionFunctions; 

public class ScanLinkCommand : ICommandExecutionHandler {
    public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        string link = cmd.GetArgument<string>("link")!;
        await cmd.RespondWithEmbedAsync("Link Scanner", "Submitting link for scanning...");

        string uuid;
        try {
            uuid = await LinkScannerService.SubmitLink(link);
        }
        catch (LinkScanRateLimitException) {
            await cmd.ModifyWithEmbedAsync("Link Scanner", "Scanning link failed: Rate limit exceeded try again later.",
                ResponseType.Error);
            return;
        }
        catch (LinkScanHttpRequestFailed e) {
            Logger.Warn(e);
            await cmd.ModifyWithEmbedAsync("Link Scanner", "Scanning link failed: Failed to connect to API.",
                ResponseType.Error);
            return;
        }
        catch (LinkScanUnknownLinkScanFailException e) {
            Logger.Warn(e);
            await cmd.ModifyWithEmbedAsync("Link Scanner", "Scanning link failed: An unknown error occured.",
                ResponseType.Error);
            return;
        }
        
        await cmd.ModifyWithEmbedAsync("Link Scanner", "Scanning link...");

        int score;
        try {
            score = await LinkScannerService.GetLinkScore(uuid);
        }
        catch (LinkScanTimeoutException) {
            await cmd.ModifyWithEmbedAsync("Link Scanner", "The API took too long to scan the link.",
                ResponseType.Error);
            return;
        }
        
        ResponseType responseType = score >= 0 ? ResponseType.Error : ResponseType.Success;
        await cmd.ModifyWithEmbedAsync("Link Scanner", $"Scan complete. Score: {score}. -100 means clean, 100 means malicious.", responseType);
    }
}