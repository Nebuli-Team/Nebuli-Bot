using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace NebuliBot.Modules.Commands
{
    public class ReloadCountCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("reload", "Reloads the server count for the bot.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ReloadCount()
        {
            await Program.StaticAccess.UpdateBotStatus();
            await RespondAsync("Updated the bot server counter to " + Program.StaticAccess.LatestServerCount);
        }

    }
}
