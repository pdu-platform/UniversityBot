using System;

namespace UniversityBot.Core.DAL
{
    public class NotFoundWord : BotEntity<NotFoundWord>
    {
        public string Question { get; private set; }
        
        public DateTime CreateTime { get; private set; }

        private NotFoundWord() : base(Guid.Empty)
        {
        }
        
        public NotFoundWord(Guid id, string question, DateTime createTime) : base(id)
        {
            Question = question;
            CreateTime = createTime;
        }

        protected override bool EqualsCore(NotFoundWord other) =>
            string.Equals(Question, other.Question) && CreateTime == other.CreateTime;

        protected override int GetHashCodeCore() => HashCode.Combine(Question ?? string.Empty, CreateTime);
    }
}