using DemocracyBot.Commands;
using DemocracyBot.EventHandlers;
using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;

namespace DemocracyBot; 

public class Bot {
    
    private static DiscordSocketClient? _client;

    public static bool IsMe(SocketUser user) {
        return user.Id == _client!.CurrentUser.Id;
    }

    public async Task Run() {
        _client = new DiscordSocketClient();
        
        // Events
        _client.Log += Log;
        _client.Ready += ClientReady;
        _client.SlashCommandExecuted += SlashCommandHandler;
        _client.MessageReceived += MessageHandler.OnMessage;

        await _client.LoginAsync(TokenType.Bot, Program.Config!["token"]);
        await _client.StartAsync();
        
        // Block this task until the program is closed.
        await Task.Delay(-1);
        Logger.Warn("Bot is shutting down");
    }
    
    private Task SlashCommandHandler(SocketSlashCommand command) {
        try {
            CommandManager.Invoke(command, _client!);
        }
        catch (Exception e) {
            Logger.Error(e);
        }
        return Task.CompletedTask;
    }
    
    private async Task ClientReady() {
        Logger.Debug("Client ready");
        await _client!.SetGameAsync("serble.net");
    }
    
    private static Task Log(LogMessage msg) {
        switch (msg.Severity) {
            case LogSeverity.Critical:
                Logger.Error(msg);
                break;
            case LogSeverity.Error:
                Logger.Error(msg);
                break;
            case LogSeverity.Warning:
                Logger.Warn(msg);
                break;
            case LogSeverity.Info:
                Logger.Info(msg);
                break;
            case LogSeverity.Verbose:
                Logger.Debug(msg);
                break;
            case LogSeverity.Debug:
                Logger.Debug(msg);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(msg));
        }

        return Task.CompletedTask;
    }
    
}