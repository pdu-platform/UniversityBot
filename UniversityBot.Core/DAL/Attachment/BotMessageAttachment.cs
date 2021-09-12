using System;

namespace UniversityBot.Core.DAL.Attachment
{
    public class BotMessageAttachment : BotAttachment<BotMessageAttachment, BotQuestioner>
    {
        protected BotMessageAttachment()
        {
        }
        
        public BotMessageAttachment(Guid id, BotQuestioner crossEntity, BotFile file) : base(id, crossEntity, file)
        {
        }

        public static BotMessageAttachment ForRemove(Guid id)
        {
            return new() { Id = id };
        }
    }
}