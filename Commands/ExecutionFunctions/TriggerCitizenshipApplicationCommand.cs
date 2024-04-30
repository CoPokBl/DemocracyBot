using Discord;
using Discord.WebSocket;
using SimpleDiscordNet.Commands;

namespace DemocracyBot.Commands.ExecutionFunctions;

public class TriggerCitizenshipApplicationCommand {

    [SlashCommand("trigger-citizen-app", "Creates a citizen application for all users or a specific user")]
    [SlashCommandArgument("user", "The user to start an application for, leave blank for all users", false, ApplicationCommandOptionType.User)]
    public async Task Execute(SocketSlashCommand cmd, DiscordSocketClient client) {
        if (!cmd.User.IsPresident()) {
            await cmd.RespondWithEmbedAsync("Citizenship", "You must be the president to use this command.",
                ResponseType.Error);
            return;
        }

        IUser? user = cmd.GetArgument<SocketGuildUser>("user");

        if (user == null) {  // Everyone
            await cmd.DeferAsync();
            IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> users = client.GetGuild(Utils.Pucv("server_id")).GetUsersAsync();
            await foreach (IReadOnlyCollection<IGuildUser> ua in users) {
                foreach (IGuildUser u in ua) {
                    if (u.IsBot) {
                        continue;
                    }
                    if (u.IsCitizen()) {  // Already a citizen
                        continue;
                    }
                    await CitizenshipManager.TriggerCitizenshipApplication(u);
                }
            }

            await cmd.ModifyWithEmbedAsync("Citizenship", "Created applications for all current guild users.");
            return;
        }
        
        if (user.IsBot) {
            await cmd.RespondWithEmbedAsync("Citizenship", "Bots cannot be citizens.",
                ResponseType.Error);
            return;
        }

        if (user.IsCitizen()) {
            await cmd.RespondWithEmbedAsync("Citizenship", "User is already a citizen.",
                ResponseType.Error);
            return;
        }

        await CitizenshipManager.TriggerCitizenshipApplication(user);
        await cmd.RespondWithEmbedAsync("Citizenship", $"Created application for {user.Mention}.", ResponseType.Success);
    }

}