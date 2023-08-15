using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace NebuliBot.Modules.Commands
{
    [Group("banmanagement", "Banning management")]
    public partial class BanCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("unban", "Unbans the specified user")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task UnbanAsync([Summary("UserID", "The userID to unban")] string id)
        {
            if (!ulong.TryParse(id, out ulong userId))
            {
                await RespondAsync("Invalid UserID", ephemeral: true);
                return;
            }
            await Context.Guild.RemoveBanAsync(userId);
            await RespondAsync(embed: await EmbedModule.CreateBasicEmbed("User Unbanned", $"{id} has been unbanned", Color.Green));
        }
    }

    public partial class BanCommand
    {
        [SlashCommand("id", "Bans the specified UserID")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanIdAsync([Summary("UserID", "The userID to ban")] string id, [Summary("Reason", "The reason of the ban")] string reason, [Summary("Prune", "The Days to Prune")] int days)
        {
            if (!ulong.TryParse(id, out ulong userId))
            {
                await RespondAsync("Invalid UserID", ephemeral: true);
                return;
            }
            
            await Context.Guild.AddBanAsync(userId, 7, reason);
            await RespondAsync(embed: await EmbedModule.CreateBasicEmbed("User Banned", $"{id} has been banned for: {reason}", Color.Orange));
        }
    }

    public partial class BanCommand
    {
        [SlashCommand("user", "Bans the specified user")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanUserAsync([Summary("User", "User to ban")] SocketUser user, [Summary("Reason", "The reason of the ban")] string reason, [Summary("Prune", "The Days to Prune")] int days)
        {
            await ((IGuildUser)user).BanAsync(days, reason);
            await RespondAsync(embed: await EmbedModule.CreateBasicEmbed("User Banned", $"{user.Username} has been banned for: {reason}", Color.Orange));
        }
    }
}