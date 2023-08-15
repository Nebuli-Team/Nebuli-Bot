using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace NebuliBot.Modules.Commands
{
    public class KickCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("kick", "Kick a user from the server")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickAsync([Summary("User", "The user to kick")] SocketUser user, [Summary("Reason", "The reason of the kick")] string reason)
        {
            await ((IGuildUser)user).KickAsync(reason);
            await RespondAsync(embed: await EmbedModule.CreateBasicEmbed("User Kicked", $"{user.Username} has been kicked for: {reason}", Color.Orange));
        }
    }
}