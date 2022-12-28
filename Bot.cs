using DemocracyBot.Commands;
using DemocracyBot.Data;
using DemocracyBot.EventHandlers;
using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;

namespace DemocracyBot; 

public class Bot {
    
    private static DiscordSocketClient? _client;
    private static CancellationTokenSource _cts;
    private static bool _hasBeenInitialized;

    public Bot() {
        if (_hasBeenInitialized) {
            Logger.Error("Bot has already been initialized!"); ;
            Logger.Error(Environment.StackTrace);
        }
        _hasBeenInitialized = true;
        _cts = new CancellationTokenSource();
    }

    public static void Reset() {
        _hasBeenInitialized = false;
        _client = null;
        _cts.Cancel();
    }

    public static bool IsMe(SocketUser user) {
        return user.Id == _client!.CurrentUser.Id;
    }

    public async Task Run() {
        DiscordSocketConfig config = new() {
            GatewayIntents = GatewayIntents.All
        };
        _client = new DiscordSocketClient(config);

        // Events
        _client.Log += Log;
        _client.Ready += ClientReady;
        _client.SlashCommandExecuted += SlashCommandHandler;
        _client.MessageReceived += MessageHandler.OnMessage;

        await _client.LoginAsync(TokenType.Bot, Program.Config!["token"]);
        await _client.StartAsync();
        
        // Block this task until the program is closed.
        await Task.Delay(-1, _cts.Token);
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
        await _client!.SetGameAsync("Fuck communism");
        TimeCheckService.StartThread(_client);
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