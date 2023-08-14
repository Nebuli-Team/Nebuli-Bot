using System.Threading.Tasks;
using Discord.Interactions;
using NebuliBot.Modules.Interactions;

namespace NebuliBot.Modules.Commands
{
    public class SubmitPluginCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("submitplugin", "Send a plugin for submission")]
        public async Task SubmitPlugin() => await RespondWithModalAsync(SubmissionModal.SendSubmissionModal);
    }
}