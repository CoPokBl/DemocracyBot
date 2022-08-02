using DemocracyBot.Commands;
using DemocracyBot.Data;
using Discord;
using Discord.WebSocket;
using GeneralPurposeLib;

namespace DemocracyBot;

internal static class Program {
    
    public const string Version = "0.0.1";
    public static Dictionary<string, string>? Config;
    public static Random Random { get; } = new ();

    private static readonly Dictionary<string, string> DefaultConfig = new() {
        { "token", "discord bot token" },
        { "testing_server_id", "911109182842602044" }
    };
    

    public static int Main(string[] args) {

        // Init Logger
        try {
            Logger.Init(LogLevel.Debug);
        }
        catch (Exception e) {
            Console.WriteLine("Failed to initialize logger: " + e);
            return 1;
        }
        Logger.Info("DemocracyBot starting...");

        // Config
        try {
            ConfigManager configManager = new ("config.json", DefaultConfig);
            Config = configManager.LoadConfig();
        }
        catch (Exception e) {
            Logger.Error("Failed to load config: " + e);
            Logger.WaitFlush();
            return 1;
        }
        
        if (args.Length != 0) {

            switch (args[0].ToLower()) {
                
                default:
                    Console.WriteLine("Unknown command");
                    return 1;
                
                case "updatecommands":
                    DiscordSocketClient client = new ();
                    bool finished = false;
                    client.Ready += () => {
                        CommandManager.UpdateCommands(client);
                        Logger.Info("Commands updated");
                        finished = true;
                        return Task.CompletedTask;
                    };
                    client.LoginAsync(TokenType.Bot, Config["token"]).Wait();
                    client.StartAsync().Wait();
                    while (!finished) {
                        Thread.Sleep(100);
                    }
                    Logger.Debug("Command execution finished");
                    Logger.WaitFlush();
                    return 0;
                
                case "updatecommand":
                    if (args.Length != 2) {
                        Logger.Info("Usage: updatecommand <command>");
                        Logger.WaitFlush();
                        return 1;
                    }
                    DiscordSocketClient updateClient = new ();
                    bool updateFinished = false;
                    updateClient.Ready += () => {
                        CommandManager.UpdateCommand(updateClient, args[1]);
                        Logger.Info("Command updated");
                        updateFinished = true;
                        return Task.CompletedTask;
                    };
                    updateClient.LoginAsync(TokenType.Bot, Config["token"]).Wait();
                    updateClient.StartAsync().Wait();
                    while (!updateFinished) {
                        Thread.Sleep(100);
                    }
                    Logger.Debug("Command execution finished");
                    Logger.WaitFlush();
                    return 0;

            }
        }
        
        // Init Services
        ServiceManager.Init();
        
        // Run bot
        List<DateTime> errors = new();
        Exception? lastError = null;
        while (true) {
            try {
                Logger.Info("Starting bot...");
                Bot bot = new ();
                bot.Run().Wait();
                Logger.Warn("Bot task exited unexpectedly");
                break;
            }
            catch (Exception e) {
                Logger.Error(e);
                
                // Stop if there are more than 5 errors in 1 minute
                // Remove all errors older than 5 minutes
                errors.Add(DateTime.Now);
                errors.RemoveAll(x => x < DateTime.Now - TimeSpan.FromMinutes(5));
                if (errors.Count > 5) {
                    Logger.Error("Too many errors (Possible error loop), stopping...");
                    break;
                }
                
                // Stop if the same error happened twice in a row
                if (lastError == e) {
                    Logger.Error("Same error twice in a row, stopping...");
                    break;
                }
                lastError = e;

                Logger.Info("Restating bot in 5 seconds...");
                Thread.Sleep(5000);
            }
        }

        Logger.WaitFlush();
        return 0;
    }
}