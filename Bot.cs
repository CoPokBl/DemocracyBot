using DemocracyBot.Data;
using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;
using SimpleDiscordNet;

namespace DemocracyBot; 

public class Bot {
    
    private static SimpleDiscordBot? _client;
    private static CancellationTokenSource _cts;
    private static bool _hasBeenInitialized;
    private static bool _hasReadied;

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
        return user.Id == _client!.Client.CurrentUser.Id;
    }

    public async Task Run(bool updateAllCommands = false, string? updateCommand = null) {
        _client = new SimpleDiscordBot(GlobalConfig.Config["token"]);
        CitizenshipManager.Discord = _client.Client;
        
        // Events
        _client.Client.Ready += ClientReady;
        _client.Client.UserJoined += CitizenshipManager.UserJoinedGuild;
        _client.Log += Log;
        
        await _client.StartBot();

        if (updateAllCommands) {
            _client.Client.Ready += async () => _client.UpdateCommands();
        }
        else if (updateCommand != null) {
            _client.Client.Ready += async () => _client.UpdateCommand(updateCommand);
        }
        
        // Block this task until the program is closed.
        await Task.Delay(-1, _cts.Token);
        Logger.Warn("Bot is shutting down");
    }
    
    private async Task ClientReady() {
        Logger.Debug("Client ready");
        await _client!.Client.SetActivityAsync(new CustomStatusGame(GlobalConfig.Config["bot_status"]));
        if (!_hasReadied) {  // Don't run these things twice
            TimeCheckService.Start(_client.Client);
        }
        _hasReadied = true;
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