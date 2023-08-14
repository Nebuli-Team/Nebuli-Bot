using System.Threading.Tasks;
using Discord;

namespace NebuliBot.Logger
{
    public interface ILogger
    {
        public Task Log(LogMessage message);
    }
}