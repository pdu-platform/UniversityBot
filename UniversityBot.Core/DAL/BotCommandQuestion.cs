using System;

namespace UniversityBot.Core.DAL
{
    public class BotCommandQuestion : BotEntity<BotCommandQuestion>
    {
        private BotCommand _command;
        
        public string Question { get; private set; }
        public Guid BotCommandId { get; private set; }

        public BotCommand BotCommand
        {
            get => _command;
            set
            {
                _command = value;
                BotCommandId = value?.Id ?? Guid.Empty;
            }
        }

        private BotCommandQuestion() : base(Guid.Empty)
        {
            
        }
        
        public BotCommandQuestion(Guid id, string question, Guid botCommandId, BotCommand botCommand) : base(id)
        {
            Question = question;
            BotCommand = botCommand;
            BotCommandId = botCommandId;
        }

        public static BotCommandQuestion ForRemove(Guid id) => new BotCommandQuestion()
        {
            Id = id
        };
        
        protected override bool EqualsCore(BotCommandQuestion other) 
            => Question == other.Question && BotCommandId == other.BotCommandId;

        protected override int GetHashCodeCore() => HashCode.Combine(Question, BotCommandId);
    }
}