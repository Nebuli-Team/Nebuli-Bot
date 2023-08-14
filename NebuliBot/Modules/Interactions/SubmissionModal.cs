using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace NebuliBot.Modules.Interactions
{
    public class SubmissionModal
    {
        private static TextInputBuilder PluginTitle { get; } = new("Title", "submission_title", TextInputStyle.Short, "Plugin title", required: true);
        
        private static TextInputBuilder PluginLink { get; } = new("Github Link", "submission_link", TextInputStyle.Short, "A link to the plugin's github repository", required: true);
        
        private static TextInputBuilder PluginDescription { get; } = new("Description", "submission_description", TextInputStyle.Paragraph, "Plugin description", required: true);

        private static ButtonBuilder DeleteButton { get; } = new("Delete", "submission_remove", ButtonStyle.Danger);

        private static ButtonBuilder AcceptButton { get; } = new("Accept", "submission_accept", ButtonStyle.Success);
        
        internal static Modal SendSubmissionModal { get; } = new ModalBuilder()
            .WithTitle("Plugin Submission")
            .WithCustomId("submission_send")
            .AddTextInput(PluginTitle)
            .AddTextInput(PluginLink)
            .AddTextInput(PluginDescription)
            .Build();

        private static Task<Embed> CreateEmbed(SocketModal modal)
        {
            var title = string.Empty;
            var repo = string.Empty;
            var description = string.Empty;
            
            foreach (SocketMessageComponentData? data in modal.Data.Components)
            {
                if (data.CustomId == PluginTitle.CustomId)
                    title = data.Value;
                else if (data.CustomId == PluginLink.CustomId)
                    repo = data.Value;
                else if (data.CustomId == PluginDescription.CustomId)
                    description = data.Value;
            }

            var submissionEmbed = new EmbedBuilder()
                .WithTitle(title)
                .WithAuthor(modal.User)
                .WithFooter(EmbedModule.Footer)
                .WithCurrentTimestamp()
                .WithColor(Color.Purple)
                .WithDescription(description)
                .AddField("Plugin Link", repo);
            
            return Task.FromResult(submissionEmbed.Build());
        }

        private static async Task HandlePluginSubmission(SocketModal modal)
        {
            var embed = await CreateEmbed(modal);

            var channel = Program.StaticAccess.Guild.GetTextChannel(1137888208315756648);
            IUserMessage msg = await channel.SendMessageAsync(embed: embed);
            await msg.ModifyAsync(x => x.Components = new ComponentBuilder().WithButton(AcceptButton).WithButton(DeleteButton).Build());
            
            await modal.RespondAsync(embed: await EmbedModule.CreateBasicEmbed("Plugin Submission Sended", "Your submission has been sended", Color.Magenta), ephemeral: true);
        }

        public static async Task HandleButton(SocketMessageComponent component)
        {
            if (component.Data.CustomId == DeleteButton.CustomId)
            {
                if (((IGuildUser) component.User).RoleIds.All(x => x != 1131406695151714386))
                {
                    await component.RespondAsync("You can't delete submissions", ephemeral: true);
                    return;
                }

                var embed = component.Message.Embeds.FirstOrDefault()!;
                
                if (Program.StaticAccess.Guild.Roles.Any(x => x.Name == embed.Title))
                {
                    var role = Program.StaticAccess.Guild.Roles.FirstOrDefault(x => x.Name == embed.Title);
                    if (role is not null)
                        await role.DeleteAsync();
                    await component.RespondAsync(embed: await EmbedModule.CreateBasicEmbed("Plugin Deleted", "The Plugin Channel & Role has been removed", Color.Green), ephemeral: true);
                    if (component.Channel.Id != 1137888208315756648)
                        await Program.StaticAccess.Guild.GetChannel(component.Channel.Id).DeleteAsync();
                }

                await component.Message.DeleteAsync();
                await component.RespondAsync(embed: await EmbedModule.CreateBasicEmbed("Plugin Submission Cancelled", "The plugin submission has been cancelled", Color.Green), ephemeral: true);
            }
            else if (component.Data.CustomId == AcceptButton.CustomId)
            {
                if (((IGuildUser) component.User).RoleIds.All(x => x != 1131406695151714386))
                    await component.RespondAsync("You can't accept submissions", ephemeral: true);
                else
                {
                    await component.DeferAsync();
                    
                    var msg = (IUserMessage)await component.Channel.GetMessageAsync(component.Message.Id);
                    var embed = (Embed)msg.Embeds.FirstOrDefault()!;
                    
                    SocketForumChannel? forumChannel = Program.StaticAccess.Guild.Channels.FirstOrDefault(x => x.Id == 1137886493604589648) as SocketForumChannel;

                    await forumChannel?.CreatePostAsync(embed.Title, ThreadArchiveDuration.ThreeDays, embed: embed, components: new ComponentBuilder().WithButton(DeleteButton).Build())!;
                    
                    await Program.StaticAccess.Guild.CreateRoleAsync(embed.Title);
                    
                    await msg.DeleteAsync();
                    
                    await component.FollowupAsync(embed: await EmbedModule.CreateBasicEmbed("Plugin Submission Accepted", "The plugin submission has been accepted", Color.Green), ephemeral: true);
                }
            }
        }
        
        public static async Task HandleModal(SocketModal modal)
        {
            string customId = modal.Data.CustomId;
            if (customId.Contains('|'))
            {
                string toRemove = modal.Data.CustomId.Substring(modal.Data.CustomId.IndexOf('|'));
                customId = modal.Data.CustomId.Replace(toRemove, string.Empty);
            }

            if (modal.Data.CustomId == SendSubmissionModal.CustomId)
                await HandlePluginSubmission(modal);
        }
    }
}