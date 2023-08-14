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
using NebuliBot.Services;

namespace NebuliBot
{
    public class Program
    {
        private DiscordSocketClient _client;
    
        public static Task Main() => new Program().MainAsync();
    
        private SocketGuild? guild;

        public SocketGuild Guild => guild ??= _client.Guilds.FirstOrDefault(g => g.Id == 1131406603791372332)!;

        public static Program StaticAccess;

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
            await RunAsync(host);
        }
        
        public async Task RunAsync(IHost host)
        {
            Console.WriteLine($"Starting NebuliBot. Version: {Assembly.GetExecutingAssembly().GetName().Version}");
        
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;
        
            var commands = provider.GetRequiredService<InteractionService>();
            _client = provider.GetRequiredService<DiscordSocketClient>();
        
            var config = provider.GetRequiredService<IConfigurationRoot>();
            await provider.GetRequiredService<InteractionCommandService>().LoadCommands();
        
            _client.UserJoined += ServerLogsModule.OnUserJoin;
            _client.UserLeft += ServerLogsModule.OnUserLeave;
            _client.RoleUpdated += ServerLogsModule.OnRoleUpdate;
            _client.ThreadCreated += ServerLogsModule.OnThreadCreated;
            _client.ThreadDeleted += ServerLogsModule.OnThreadDeleted;
            _client.MessageDeleted += ServerLogsModule.OnMessageDeleted;
            _client.MessageUpdated += ServerLogsModule.OnMessageUpdated;
            _client.InviteCreated += ServerLogsModule.OnInviteCreated;
            _client.InviteDeleted += ServerLogsModule.OnInviteDeleted;
            _client.UserBanned += ServerLogsModule.OnUserBanned;
            _client.UserUnbanned += ServerLogsModule.OnUserUnbanned;

            _client.Log += _ => provider.GetRequiredService<ConsoleLogger>().Log(_);
            commands.Log += _ => provider.GetRequiredService<ConsoleLogger>().Log(_);

            _client.Ready += async () =>
            {
                await _client.SetStatusAsync(UserStatus.DoNotDisturb);
                await _client.SetActivityAsync(new Game("Discord Utility Bot", ActivityType.Listening));
                
                Console.WriteLine($"Conected as => {_client.CurrentUser}");
            
                await commands.RegisterCommandsToGuildAsync(UInt64.Parse(config["guild"]), true);
            };
        
            await _client.LoginAsync(TokenType.Bot, config["token"]);
            await _client.StartAsync();
            
            await Task.Delay(-1);
        }
    }
}