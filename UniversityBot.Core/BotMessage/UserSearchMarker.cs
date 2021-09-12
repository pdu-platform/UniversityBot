using UniversityBot.Core.Enums;

namespace UniversityBot.Core.BotMessage
{
    public sealed class UserSearchMarker : IBotMessage
    {
        public static readonly IBotMessage Instance = new UserSearchMarker();
        
        public BotMessageType Type => BotMessageType.UserSearch;
        public string HandlerName => null;

        private UserSearchMarker() {}
    }
}