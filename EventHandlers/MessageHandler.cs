using Discord;
using Discord.WebSocket;

namespace DemocracyBot.EventHandlers; 

public static class MessageHandler {

    public static Task OnMessage(SocketMessage msg) {

        if (Bot.IsMe(msg.Author)) {
            return Task.CompletedTask;
        }
        
        if (msg.Channel is IDMChannel) {
            DmHandler.Run(msg);
        }

        return Task.CompletedTask;
    }
    
}