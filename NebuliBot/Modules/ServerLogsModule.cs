using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace NebuliBot.Modules
{
    public class ServerLogsModule
    {
        private static HttpClient _httpClient = new();
    
        internal static async Task OnUserJoin(SocketGuildUser user)
        {
            // Welcome Message
            var welcomeEmbed = new EmbedBuilder()
                .WithAuthor(user.Nickname ?? user.Username, user.GetAvatarUrl())
                .WithDescription($"Welcome to Nebuli {user.Mention}")
                .WithColor(Color.Green)
                .WithTitle("New Member")
                .WithCurrentTimestamp()
                .WithFooter(EmbedModule.Footer);

            await Program.StaticAccess.Guild.GetTextChannel(1137212602632650782).SendMessageAsync(embed: welcomeEmbed.Build());
        
            // Log Message
            var users = Program.StaticAccess.Guild.Users;

            var logEmbed = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithAuthor($"New Member - {user.Username}", user.GetAvatarUrl())
                .WithDescription("**User:** " + user.Mention + "\n**ID:** " + user.Id + "\n**Creation Time:** " +
                                 user.CreatedAt.ToString("MM/dd/yyyy") + "\n**Date:** " +
                                 DateTime.Now.ToString("MM/dd/yyyy") + "\n**Total Count:** " + users.Count)
                .WithFooter(EmbedModule.Footer)
                .WithCurrentTimestamp();

            await Program.StaticAccess.Guild.GetTextChannel(1137219362340745319).SendMessageAsync(embed: logEmbed.Build());
        
            // Role Assignment
            var role = Program.StaticAccess.Guild.GetRole(1133158458775044217);
            await user.AddRoleAsync(role);
        }
        
        internal static async Task OnUserLeave(SocketGuild guild, SocketUser user)
        {
            // Log Message
            var users = Program.StaticAccess.Guild.Users;
        
            var logEmbed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithAuthor($"Member Left - {user.Username}", user.GetAvatarUrl())
                .WithDescription("**User:** " + user.Mention + "\n**ID:** " + user.Id + "\n**Date:** " +
                                 DateTime.Now.ToString("MM/dd/yyyy") + "\n**Total Count:** " + users.Count)
                .WithFooter(EmbedModule.Footer)
                .WithCurrentTimestamp();
        
            await Program.StaticAccess.Guild.GetTextChannel(1137219362340745319).SendMessageAsync(embed: logEmbed.Build());
        }

        internal static async Task OnRoleUpdate(SocketRole oldRole, SocketRole newRole)
        {
            // Log Message
            var sb = new StringBuilder();
            foreach (var propertyInfo in typeof(GuildPermissions).GetProperties())
            {
                if(propertyInfo.GetValue(oldRole.Permissions)?.GetType() != typeof(bool))
                    continue;
                if (propertyInfo.GetValue(oldRole.Permissions) as bool? != propertyInfo.GetValue(newRole.Permissions) as bool?)
                    sb.AppendLine("**Changes:**\n Permission " + propertyInfo.Name + " Changed from " +
                                  propertyInfo.GetValue(oldRole.Permissions) + " to " +
                                  propertyInfo.GetValue(newRole.Permissions));
            }
        
            var logEmbed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithAuthor("Role Updated")
                .WithFooter(EmbedModule.Footer)
                .WithCurrentTimestamp()
                .WithDescription(
                    $"**Role:** {newRole.Name}\n**ID:** {newRole.Id}\n**Date:** {DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}\n" +
                    $"{(oldRole.Name != newRole.Name ? $"**Previous Name:** {oldRole.Name}\n**New Name:** {newRole.Name}\n" : "")}" +
                    $"{(oldRole.Color != newRole.Color ? $"**Previous Color:** {oldRole.Color}\n**New Color:** {newRole.Color}\n" : "")}" +
                    $"{(oldRole.Position != newRole.Position ? $"**Previous Position:** {oldRole.Position}\n**New Position:** {newRole.Position}\n" : "")}" +
                    $"{(oldRole.IsMentionable != newRole.IsMentionable ? $"**Is Mentionable:** {oldRole.IsMentionable}\n**Is Mentionable:** {newRole.IsMentionable}\n" : "")}" +
                    $"{(oldRole.IsHoisted != newRole.IsHoisted ? $"**Previous Hoisted:** {oldRole.IsHoisted}\n**New Hoisted:** {newRole.IsHoisted}\n" : "")}" + 
                    $"{(oldRole.Permissions.RawValue != newRole.Permissions.RawValue ? sb.ToString() : "")}");
        
            await Program.StaticAccess.Guild.GetTextChannel(1137219362340745319).SendMessageAsync(embed: logEmbed.Build());
        }
        
        internal static async Task OnThreadCreated(SocketThreadChannel thread)
        {
            // Log Message
            var logEmbed = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithAuthor("Thread Created")
                .WithDescription("**Thread Name:** " + thread.Name + "\n**Author:** " + thread.Owner.Mention +
                                 "\n**Channel:** " + thread.ParentChannel.Name + "\n**Date:** " +
                                 DateTime.Now.ToString("MM/dd/yyyy"))
                .WithFooter(EmbedModule.Footer)
                .WithCurrentTimestamp();
        
            await Program.StaticAccess.Guild.GetTextChannel(1137219362340745319).SendMessageAsync(embed: logEmbed.Build());
        }
        
        internal static async Task OnThreadDeleted(Cacheable<SocketThreadChannel, ulong> thread)
        {
            // Log Message
            var logEmbed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithAuthor("Thread Deleted")
                .WithDescription("**Thread Name:** " + thread.Value.Name + "\n**Author:** " + thread.Value.Owner.Mention +
                                 "\n**Channel:** " + thread.Value.ParentChannel.Name + "\n**Date:** " +
                                 DateTime.Now.ToString("MM/dd/yyyy"))
                .WithFooter(EmbedModule.Footer)
                .WithCurrentTimestamp();
        
            await Program.StaticAccess.Guild.GetTextChannel(1137219362340745319).SendMessageAsync(embed: logEmbed.Build());
        }

        internal static async Task OnMessageDeleted(Cacheable<IMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2)
        {
            // Log Message
            if (!arg1.HasValue || !arg2.HasValue || arg1.Value.Author.IsBot) return;
        
            var logEmbed = new EmbedBuilder()
                .WithAuthor("Message Deleted - " + arg1.Value.Author.Username)
                .WithColor(Color.DarkRed)
                .AddField("Channel", $"<#{arg2.Id}> ({arg2.Id})")
                .AddField("Author", arg1.Value.Author.Mention)
                .WithFooter(EmbedModule.Footer)
                .WithCurrentTimestamp()
                .AddField("Created At",
                    $"<t:{new DateTimeOffset(arg1.Value.CreatedAt.DateTime.ToLocalTime()).ToUnixTimeSeconds()}:R>")
                .AddField("Bot", arg1.Value.Author.IsBot ? "Yes" : "No");
            if (!string.IsNullOrEmpty(arg1.Value.Content))
                logEmbed.WithDescription("**Content:**\n" + (arg1.Value.Content.Contains("```") ? "" : "```")
                                                          + arg1.Value.Content +
                                                          (arg1.Value.Content.Contains("```") ? "" : "```"));
            
            if (arg1.Value.Attachments != null && arg1.Value.Attachments.Any())
            {
                var mediaFiles = arg1.Value.Attachments?.Where(x =>
                    x.Filename.ToLower().EndsWith(".png") || x.Filename.ToLower().EndsWith(".jpg") ||
                    x.Filename.ToLower().EndsWith(".jpeg") || x.Filename.ToLower().EndsWith(".gif"));
            
                if (mediaFiles != null && mediaFiles.Any())
                {
                    if (mediaFiles.Count() > 1)
                    {
                        var urls = new StringBuilder();
                        foreach (var image in mediaFiles)
                            urls.AppendLine(await CreateMediaFile(image.ProxyUrl));
                        logEmbed.AddField("Media Files", urls);
                    }
                    else
                        logEmbed.WithImageUrl(await CreateMediaFile(arg1.Value.Attachments?.First()?.ProxyUrl!));
                }
            }
        
            await Program.StaticAccess.Guild.GetTextChannel(1137219362340745319).SendMessageAsync(embed: logEmbed.Build());
        }
        
        private static async Task<string?> CreateMediaFile(string url)
        {
            return JsonConvert.DeserializeObject<ImgBbResponse>(await _httpClient.GetStringAsync(
                    $"NO"))
                ?.Data.Url;
        }

        internal static async Task OnMessageUpdated(Cacheable<IMessage, ulong> cacheable, SocketMessage msg, ISocketMessageChannel messageChannel)
        {
            // Log Message
            if (!cacheable.Value.Author.IsBot && cacheable.HasValue)
            {
                var logEmbed = new EmbedBuilder()
                    .WithColor(Color.Teal)
                    .WithAuthor($"Edited Message - {msg.Author.Username}", msg.Author.GetAvatarUrl())
                    .AddField("Channel", $"<#{messageChannel.Id}> ({messageChannel.Id})")
                    .AddField("Author", msg.Author.Mention)
                    .AddField("Created At",
                        $"<t:{new DateTimeOffset(msg.CreatedAt.DateTime.ToLocalTime()).ToUnixTimeSeconds()}:R>")
                    .AddField("Edited At", $"<t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:R>")
                    .WithFooter(EmbedModule.Footer)
                    .AddField("Bot", msg.Author.IsBot ? "Yes" : "No");

                var stringBuilder = new StringBuilder();
            
                if (!string.IsNullOrEmpty(cacheable.Value.Content))
                {
                    stringBuilder.Append("**Previous Message**\n");
                    stringBuilder.Append(cacheable.Value.Content.Contains("```") ? "" : "```");
                    stringBuilder.Append(cacheable.Value.Content);
                    stringBuilder.Append(cacheable.Value.Content.Contains("```") ? "" : "```");
                }
                if (!string.IsNullOrEmpty(msg.Content))
                {
                    stringBuilder.Append("**New Message**\n");
                    stringBuilder.Append(msg.Content.Contains("```") ? "" : "```");
                    stringBuilder.Append(msg.Content);
                    stringBuilder.Append(msg.Content.Contains("```") ? "" : "```");
                }

                logEmbed.WithDescription(stringBuilder.ToString());
                var button = new ComponentBuilder().WithButton("Go to the Message", style: ButtonStyle.Link, url: msg.GetJumpUrl());

                await Program.StaticAccess.Guild.GetTextChannel(1137219362340745319).SendMessageAsync(embed: logEmbed.Build(), components: button.Build());
            }
        }
        
        internal static async Task OnInviteCreated(SocketInvite invite)
        {
            // Log Message
            var logEmbed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithAuthor("Invite Created")
                .WithDescription(
                    $"**Author:** {invite.Inviter.Mention}\n**Channel:** {invite.Channel.Name}\n**Code:** {invite.Code}\n**Date:** {DateTime.Now.ToString("MM/dd/yyyy")}")
                .WithFooter(EmbedModule.Footer)
                .WithCurrentTimestamp();
        
            await Program.StaticAccess.Guild.GetTextChannel(1137219362340745319).SendMessageAsync(embed: logEmbed.Build());
        }
        
        internal static async Task OnInviteDeleted(SocketGuildChannel channel, string invCode)
        {
            // Log Message
            var logEmbed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithAuthor("Invite Deleted")
                .WithDescription(
                    $"**Channel:** {channel.Name}\n**Code:** {invCode}\n**Date:** {DateTime.Now.ToString("MM/dd/yyyy")}")
                .WithFooter(EmbedModule.Footer)
                .WithCurrentTimestamp();
        
            await Program.StaticAccess.Guild.GetTextChannel(1137219362340745319).SendMessageAsync(embed: logEmbed.Build());
        }
        
        internal static async Task OnUserBanned(SocketUser socketUser, SocketGuild socketGuild)
        {
            // Log Message
            var logEmbed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithAuthor("User Banned", socketUser.GetAvatarUrl())
                .WithDescription(
                    $"**User:** {socketUser.Mention}\n**ID:** {socketUser.Id}\n**Date:** {DateTime.Now.ToString("MM/dd/yyyy")}")
                .WithFooter(EmbedModule.Footer)
                .WithCurrentTimestamp();
        
            await Program.StaticAccess.Guild.GetTextChannel(1137219362340745319).SendMessageAsync(embed: logEmbed.Build());
        }

        internal static async Task OnUserUnbanned(SocketUser socketUser, SocketGuild socketGuild)
        {
            // Log Message
            var embed = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithAuthor("User Unbanned", socketUser.GetAvatarUrl())
                .WithDescription(
                    $"**User:** {socketUser.Mention}\n**ID:** {socketUser.Id}\n**Date:** {DateTime.Now.ToString("MM/dd/yyyy")}")
                .WithFooter(EmbedModule.Footer)
                .WithCurrentTimestamp();
        
            await Program.StaticAccess.Guild.GetTextChannel(1137219362340745319).SendMessageAsync(embed: embed.Build());
        }
    }
    
    public class ImbBbResponseData
    {
        public string Url { get; set; }
    }
    
    public class ImgBbResponse
    {
        public ImbBbResponseData Data { get; set; }
    }
}