using System;

namespace UniversityBot.Core.DAL
{
    public class BotSettings : BotEntity<BotSettings>
    {
        public uint MinKeywordMatchCount { get; private set; }
        public bool OnlyAnswerWithMaxMatch { get; init; }
        public bool DontDisplayChildAndParentQuestionInAnswer { get; init; }
        public bool SplitWelcomeMessage { get; init; }
        public string NotFoundAnswerMessage { get; init; } = "Мммм... даже не знаю.";
        public bool UseFuzzySearch { get; init; }
        
        private BotSettings(Guid id)
            : base(id)
        {
        }
        
        public BotSettings(Guid id, uint minKeywordMatchCount) : base(id)
        {
            if (!TrySetMinKeywordMatchCount(minKeywordMatchCount, out var error))
                throw new ArgumentException(error, nameof(minKeywordMatchCount));
        }

        public bool TrySetMinKeywordMatchCount(uint minKeywordMatchCount, out string error)
        {
            if (!ValidateMinKeywordMatchCount(minKeywordMatchCount, out error))
                return false; 

            MinKeywordMatchCount = minKeywordMatchCount;
            return true;
        }

        public static bool ValidateMinKeywordMatchCount(uint minKeywordMatchCount, out string error)
        {
            if (minKeywordMatchCount == 0)
            {
                error = "Min value can't be 0";
                return false;
            }

            error = null;
            return true;
        }
        
        protected override bool EqualsCore(BotSettings other)
            => BuildKey() == other.BuildKey();

        protected override int GetHashCodeCore()
            => BuildKey().GetHashCode();

        private (uint, bool, bool, bool, string) BuildKey() => (MinKeywordMatchCount, OnlyAnswerWithMaxMatch,
            DontDisplayChildAndParentQuestionInAnswer, SplitWelcomeMessage, NotFoundAnswerMessage);
        
        public static BotSettings CreateDefault(Guid id = default) => new(id, 1);
    }
}