using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NebuliBot.Logger;
using NebuliBot.Modules;
using NebuliBot.Modules.Interactions;
using NebuliBot.Services;

namespace NebuliBot
{
    public class Program
    {
        private DiscordSocketClient _client;
        private ServerListService _serverListService;

        public static Task Main() => new Program().MainAsync();

        private SocketGuild? guild;

        public SocketGuild Guild => guild ??= _client.Guilds.FirstOrDefault(g => g.Id == 1131406603791372332)!;

        public static Program StaticAccess;

        public int LatestServerCount = 0;

        public Program()
        {
            StaticAccess = this;
        }

        public async Task MainAsync()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile("config.yml")
                .Build();

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                    services
                        .AddSingleton(config)
                        .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                        {
                            GatewayIntents = GatewayIntents.All,
                            AlwaysDownloadUsers = true,
                            LogLevel = LogSeverity.Debug,
                            MessageCacheSize = 5000,
                        }))
                        .AddMemoryCache()
                        .AddTransient<ConsoleLogger>()
                        .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                        .AddSingleton<InteractionCommandService>()
                        .AddSingleton(new CommandService(new CommandServiceConfig
                        {
                            LogLevel = LogSeverity.Debug,
                            DefaultRunMode = Discord.Commands.RunMode.Async
                        })))
                .Build();

            _serverListService = new ServerListService();
            await RunAsync(host);
        }

        public async Task RunAsync(IHost host)
        {
            Console.WriteLine($"Starting NebuliBot. Version: {Assembly.GetExecutingAssembly().GetName().Version}");

            using IServiceScope serviceScope = host.Services.CreateScope();
            var commands = serviceScope.ServiceProvider.GetRequiredService<InteractionService>();
            _client = serviceScope.ServiceProvider.GetRequiredService<DiscordSocketClient>();

            var config = serviceScope.ServiceProvider.GetRequiredService<IConfigurationRoot>();
            await serviceScope.ServiceProvider.GetRequiredService<InteractionCommandService>().LoadCommands();

            _client.Log += _ => serviceScope.ServiceProvider.GetRequiredService<ConsoleLogger>().Log(_);
            commands.Log += _ => serviceScope.ServiceProvider.GetRequiredService<ConsoleLogger>().Log(_);

            _client.Ready += async () =>
            {
                await _client.SetStatusAsync(UserStatus.Online);
                Console.WriteLine($"Connected as => {_client.CurrentUser}");

                await commands.RegisterCommandsToGuildAsync(ulong.Parse(config["guild"]), true);

                var statusUpdateInterval = TimeSpan.FromMinutes(20);
                var statusUpdateTimer = new System.Threading.Timer(
                    async (_) => await UpdateBotStatus(),
                    null,
                    TimeSpan.Zero,
                    statusUpdateInterval);
            };

            await _client.LoginAsync(TokenType.Bot, config["token"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        public async Task UpdateBotStatus()
        {
            int serverCount = await _serverListService.GetTotalCountAsync("Nebuli");
            LatestServerCount = serverCount;
            Console.WriteLine("New server count : " + serverCount);
            if (serverCount >= 0)
            {
                await _client.SetActivityAsync(new Game($"on {serverCount} servers.", ActivityType.Playing));
            }
            else
            {
                await _client.SetActivityAsync(new Game($"on 0 servers.", ActivityType.Playing));
            }
        }
    }
}
