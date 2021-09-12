using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using UniversityBot.Core.BotMessage;
using UniversityBot.Core.Enums;

namespace UniversityBot.Infrastructure.Extension
{
    public static class MessageActivityExt
    {
        public static IBotMessage ToBotMessage(this IMessageActivity self)
        {
            if (self.Value is not JObject obj) 
                return null;
            
            try
            {
                var typeAsInt = obj.Value<int>(nameof(IBotMessage.Type));
                var castType = (BotMessageType)typeAsInt;
                switch (castType)
                {
                    case BotMessageType.Question:
                    case BotMessageType.QuestionBack:
                    case BotMessageType.Command:
                        return obj.ToObject<BotMessageMarker>();
                    case BotMessageType.ReactionKeyword:
                        return obj.ToObject<BotReactionKeywordMarker>();
                    case BotMessageType.UserSearch:
                        return UserSearchMarker.Instance;
                    case BotMessageType.None:
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}