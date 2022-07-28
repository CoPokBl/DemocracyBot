using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace SerbleBot.Commands.ExecutionFunctions; 

public class ClearCommand : ICommandExecutionHandler {
    public async void Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        int messageCount = (int)cmd.GetArgument<long?>("count")!;

        await cmd.DeferAsync();

        if (messageCount > 1000) {
            await cmd.ModifyWithEmbedAsync("Error", "You cannot delete more than 1000 messages at once.", ResponseType.Error);
            return;
        }

        int messagesCount;
        try {
            IEnumerable<IMessage> messages = 
                (await cmd.Channel
                    .GetMessagesAsync(messageCount + 1)
                    .FlattenAsync())
                    .Skip(1)
                    .Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);
            IEnumerable<IMessage> enumeratedMessages = messages as IMessage[] ?? messages.ToArray();
            messagesCount = enumeratedMessages.Count();
            
            if (cmd.Channel is not ITextChannel textChannel) {
                await cmd.ModifyWithEmbedAsync("Usage", "You can only clear messages in non DM text channels.", ResponseType.Error);
                return;
            }
            
            // Mass delete
            await textChannel.DeleteMessagesAsync(enumeratedMessages);
        }
        catch (HttpException) {
            await cmd.ModifyWithEmbedAsync("Error", "An Error Occured, do I have permission to delete messages?", ResponseType.Error);
            return;
        }

        await cmd.ModifyWithEmbedAsync("Cleared Messages", 
            $"Deleted {messagesCount} message{(messagesCount == 1 ? "" : "s")}.", ResponseType.Success);
    }
}