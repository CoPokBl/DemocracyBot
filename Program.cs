﻿using DemocracyBot.Data.Storage;
using GeneralPurposeLib;

namespace DemocracyBot;

internal static class Program {
    
    public const string Version = "0.0.1";
    public static Random Random { get; } = new ();
    public static IStorageService StorageService { get; private set; } = null!;

    private static readonly Dictionary<string, Property> DefaultConfig = new() {
        { "token", "discord bot token" },
        { "testing_server_id", "911109182842602044" },
        { "storage_service", "file" },
        { "server_id", "" },
        { "announcements_channel_id", "" },
        { "citizenship_vote_channel", "" },
        { "president_role_id", "" },
        { "citizen_role", "" },
        { "save_data", false },
        { "poll_time", 48D },
        { "term_length", 336D },
        { "bot_status", "Fuck communism" }
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
            Config config = new(DefaultConfig);
            GlobalConfig.Init(config);
        }
        catch (Exception e) {
            Logger.Error("Failed to load config: " + e);
            Logger.WaitFlush();
            return 1;
        }
        
        // Storage
        switch (GlobalConfig.Config["storage_service"].Text) {
            case "file":
                StorageService = new SqliteStorageService();
                break;
            
            default:
                Logger.Error("Unknown storage service: " + GlobalConfig.Config["storage_service"]);
                Logger.WaitFlush();
                return 1;
        }

        try {
            StorageService.Init();
        }
        catch (Exception e) {
            Logger.Error("Failed to initialize storage service: " + e);
            Logger.WaitFlush();
            return 1;
        }
        
        // This event gets fired when the user tried to stop the bot with Ctrl+C or similar.
        Console.CancelKeyPress += (_, _) => {
            Logger.Info("Shutting down...");
            if (GlobalConfig.Config["save_data"] == true) {
                try {
                    Logger.Info("Saving data...");
                    StorageService.Deinit();
                }
                catch (Exception exception) {
                    Logger.Error("Failure during storage service deinit: " + exception);
                }
            }
            else {
                Logger.Info("Data will not be saved, to change this modify 'save_data' in the config file.");
            }
            Logger.WaitFlush();
            Environment.Exit(0);
        };

        string? firstArg = args.Length != 0 ? args[0] : null;
        
        // Run bot
        List<DateTime> errors = new();
        Exception? lastError = null;
        while (true) {
            Bot bot = new();
            try {
                Logger.Info("Starting bot...");
                bot.Run(firstArg == "updatecommands", firstArg == "updatecommand" && args.Length > 1 ? args[1] : null).Wait();
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
            Bot.Reset();
        }

        Logger.WaitFlush();
        return 0;
    }
}