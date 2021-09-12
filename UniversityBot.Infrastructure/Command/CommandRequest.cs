using Dawn;
using UniversityBot.Core.BotMessage;

namespace UniversityBot.Infrastructure.Command
{
    public readonly struct CommandRequest
    {
        public string Command { get; }
        public IBotMessage Message { get; }


        public CommandRequest(string command, IBotMessage message = null)
        {
            Command = Guard.Argument(command, nameof(command)).NotNull();
            Message = message;
        }
    }
}