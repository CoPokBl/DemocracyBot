using Discord;
using Discord.WebSocket;
using SimpleDiscordNet.DMs;

namespace DemocracyBot.EventHandlers; 

public class MessageHandler {

    [DmListener]
    public Task OnMessage(SocketMessage msg) {
        if (Bot.IsMe(msg.Author)) {
            return Task.CompletedTask;
        }
        
        if (msg.Channel is IDMChannel) {
            DmHandler.Run(msg);
        }

        return Task.CompletedTask;
    }
    
}