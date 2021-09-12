using System;
using System.Collections.Generic;
using UniversityBot.Core.DAL.Attachment;

namespace UniversityBot.Core.DAL
{
    public class BotQuestioner : BotEntity<BotQuestioner>
    {
        public Guid? ParentId { get; set; }
        public BotQuestioner Parent { get; private set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        
        public List<BotMessageAttachment> Attachments { get; private set; }

        public List<BotReactionKeyword> ReactionKeywords { get; private set; }
        
        private BotQuestioner() : this(Guid.Empty, null, string.Empty, string.Empty)
        {
        }

        public BotQuestioner(Guid id, BotQuestioner parent, string question, string answer)
            : base(id)
        {
            Question = question;
            Answer = answer;
            ReactionKeywords = new List<BotReactionKeyword>();
            Attachments = new List<BotMessageAttachment>();
            ChangeParent(parent);
        }
        
        public void ChangeParent(BotQuestioner parent)
        {
            ParentId = parent?.Id;
            Parent = parent;
        }

        protected override bool EqualsCore(BotQuestioner other) => ParentId == other.ParentId && Question == other.Question;

        protected override int GetHashCodeCore() => HashCode.Combine(ParentId, Question);
    }
}