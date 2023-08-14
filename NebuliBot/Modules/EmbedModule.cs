using System.Reflection;
using System.Threading.Tasks;
using Discord;

namespace NebuliBot.Modules
{
    public static class EmbedModule
    {
        public static async Task<Embed> CreateBasicEmbed(string title, string description, Color color)
        {
            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color)
                .WithFooter(Footer)
                .WithCurrentTimestamp().Build()));
            return embed;
        }
        public static async Task<Embed> CreateErrorEmbed(string source, string error)
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                .WithTitle($"Exception in {source}")
                .WithDescription($"**Error Details**: \n{error}")
                .WithColor(Color.DarkRed)
                .WithFooter(Footer)
                .WithCurrentTimestamp().Build());
            return embed;
        }
        public static string Footer => $"Nebuli | {Assembly.GetExecutingAssembly().GetName().Version} | - Nebuli Dev Team";
    }
}