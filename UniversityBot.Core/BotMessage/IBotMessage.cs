using UniversityBot.Core.Enums;

namespace UniversityBot.Core.BotMessage
{
    public interface IBotMessage
    {
        BotMessageType Type { get; }
        string HandlerName { get; }
    }
}