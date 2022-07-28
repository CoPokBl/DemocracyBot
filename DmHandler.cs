using Discord.WebSocket;
using SerbleBot.Data;

namespace SerbleBot;

public static class DmHandler {

    public static async void Run(SocketMessage msg) {
        await msg.Channel.SendMessageAsync("Go away");
    }

}
