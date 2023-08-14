using System;
using System.Threading.Tasks;
using Discord;

namespace NebuliBot.Logger
{
    public class ConsoleLogger : LoggerBase
    {
        public override async Task Log(LogMessage message)
        {
            await Task.Run(() => LogToConsoleAsync(this, message));
        }

        private async Task LogToConsoleAsync<T>(T logger, LogMessage message) where T : ILogger
        {
            Console.WriteLine($"guid:{_guid} : " + message);
        }
    }
}