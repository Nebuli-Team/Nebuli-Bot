using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace NebuliBot.Modules.Interactions
{
    public class BugReportModal
    {
        private static TextInputBuilder BugTitle { get; } = new("Title", "bugreport_title", TextInputStyle.Short, "Bug title", required: true);
        
        private static TextInputBuilder BugDescription { get; } = new("Description", "bugreport_description", TextInputStyle.Paragraph, "Bug description", required: true);
        
        private static TextInputBuilder BugReproduction { get; } = new("How to reproduce", "bugreport_reproduction", TextInputStyle.Paragraph, "Bug reproduction", required: true);
        
        internal static Modal SendBugModal { get; } = new ModalBuilder()
            .WithTitle("Bug Report")
            .WithCustomId("bugreport_send")
            .AddTextInput(BugTitle)
            .AddTextInput(BugDescription)
            .AddTextInput(BugReproduction)
            .Build();

        private static Task<Embed> CreateEmbed(SocketModal modal)
        {
            var title = string.Empty;
            var description = string.Empty;
            var reproduction = string.Empty;
            
            foreach (SocketMessageComponentData? data in modal.Data.Components)
            {
                if (data.CustomId == BugTitle.CustomId)
                    title = data.Value;
                else if (data.CustomId == BugDescription.CustomId)
                    description = data.Value;
                else if (data.CustomId == BugReproduction.CustomId)
                    reproduction = data.Value;
            }

            var bugEmbed = new EmbedBuilder()
                .WithTitle(title)
                .WithAuthor(modal.User)
                .WithFooter(EmbedModule.Footer)
                .WithCurrentTimestamp()
                .WithColor(Color.Purple)
                .WithDescription(description)
                .AddField("How to reproduce", reproduction);
            
            return Task.FromResult(bugEmbed.Build());
        }
        
        private static async Task HandleBugReport(SocketModal modal)
        {
            var embed = await CreateEmbed(modal);

            await modal.DeferAsync();

            SocketForumChannel? forumChannel = Program.StaticAccess.Guild.Channels.FirstOrDefault(x => x.Id == 1141019584124686409) as SocketForumChannel;

            await forumChannel?.CreatePostAsync(embed.Title, ThreadArchiveDuration.ThreeDays, embed: embed)!;

            await modal.FollowupAsync(
                embed: await EmbedModule.CreateBasicEmbed("Bug Report Sended",
                    "The bug report has been sended to the bug-reports forum", Color.Magenta), ephemeral: true);
        }
        
        public static async Task HandleModal(SocketModal modal)
        {
            if (modal.Data.CustomId == SendBugModal.CustomId)
                await HandleBugReport(modal);
        }
    }
}