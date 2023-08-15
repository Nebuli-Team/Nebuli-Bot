using System.Threading.Tasks;
using Discord.Interactions;
using NebuliBot.Modules.Interactions;

namespace NebuliBot.Modules.Commands
{
    public class BugReportCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("bugreport", "Report a bug")]
        public async Task BugReportAsync() => await RespondWithModalAsync(BugReportModal.SendBugModal);
    }
}