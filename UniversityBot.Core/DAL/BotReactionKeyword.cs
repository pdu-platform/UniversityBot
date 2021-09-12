using System;

namespace UniversityBot.Core.DAL
{
    public class BotReactionKeyword : BotEntity<BotReactionKeyword>
    {
        public string Word { get; private set; }

        public Guid QuestionerId { get; private set; }
        
        public BotQuestioner Questioner { get; private set; }
        
        public ulong Hash { get; private set; }
        
        public bool Required { get; set; }
        
        private BotReactionKeyword() : base(Guid.Empty)
        {
        }
        
        public BotReactionKeyword(Guid id, Keyword keyword, Guid questionerId, BotQuestioner questioner, bool required) : base(id)
        {
            Word = keyword.Word;
            QuestionerId = questionerId;
            Questioner = questioner;
            Required = required;
            Hash = keyword.Hash;
        }

        protected override bool EqualsCore(BotReactionKeyword other)
            => BuildKey() == other.BuildKey();

        protected override int GetHashCodeCore()
            => BuildKey().GetHashCode();

        private (string, Guid, ulong, bool) BuildKey() => (Word, QuestionerId, Hash, Required);
    }
}