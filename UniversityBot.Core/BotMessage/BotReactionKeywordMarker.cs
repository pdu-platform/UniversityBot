using System;
using UniversityBot.Core.Enums;

namespace UniversityBot.Core.BotMessage
{
    public sealed class BotReactionKeywordMarker : IBotMessage
    {
        public BotMessageType Type => BotMessageType.ReactionKeyword;
        public string HandlerName => null;
        
        public Guid[] BotMessageIds { get; }

        public BotReactionKeywordMarker(Guid[] botMessageIds)
        {
            BotMessageIds = botMessageIds;
        }
    }
}