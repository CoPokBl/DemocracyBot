using Discord.WebSocket;

namespace DemocracyBot;

public static class DmHandler {

    public static async void Run(SocketMessage msg) {
        await msg.Channel.SendMessageAsync("Go away");
    }

}
