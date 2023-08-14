using System.Threading.Tasks;
using Discord;
using static System.Guid;

namespace NebuliBot.Logger
{
    public abstract class LoggerBase : ILogger
    {
        public string _guid;
        public LoggerBase()
        {
            _guid = NewGuid().ToString()[^4..];
        }

        public abstract Task Log(LogMessage message);
    }
}