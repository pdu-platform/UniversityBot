using System;
using UniversityBot.Core.Enums;

namespace UniversityBot.Core.BotMessage
{
    public sealed class BotMessageMarker : IBotMessage
    {
        public Guid Id { get; private set; }
         
        public BotMessageType Type { get; private set; }

        public string HandlerName { get; private set; }

        public BotMessageMarker(Guid id, BotMessageType type, string handlerName)
        {
            if(type == BotMessageType.None)
                throw new ArgumentException();

            Id = id;
            Type = type;
            HandlerName = handlerName;
        }

        public static IBotMessage Question(Guid id) => new BotMessageMarker(id, BotMessageType.Question, Constants.Command.Question);
        public static IBotMessage BackQuestion(Guid id) => new BotMessageMarker(id, BotMessageType.QuestionBack, Constants.Command.Question);
        public static IBotMessage Command(Guid id) => new BotMessageMarker(id, BotMessageType.Command, null);
        public static IBotMessage ReactionKeyword(Guid[] listId) => new BotReactionKeywordMarker(listId);
    }
}