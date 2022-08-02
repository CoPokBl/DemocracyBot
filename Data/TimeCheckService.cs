using Discord.WebSocket;

namespace DemocracyBot.Data; 

public class TimeCheckService {

    public static void StartThread(DiscordSocketClient client) {
            new Thread(() => {
            while (true) {
                Thread.Sleep(60000);
                Update(client);
            }
            }).Start();
    }

    public static void Update(DiscordSocketClient client) {
        
    }
    
}