using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityBot.Core.DAL;
using UniversityBot.Core.DAL.Attachment;

namespace UniversityBot.EF.Configuration
{
    public class BotMessageAttachmentConfiguration : BotAttachmentConfiguration<BotMessageAttachment, BotQuestioner>
    {
        protected override string EntityName => "tbx_Bot_Message_Attachments";
        
        protected override void ConfigureEntity(EntityTypeBuilder<BotMessageAttachment> builder)
        {
            base.ConfigureEntity(builder);
            
            builder
                .HasOne(e => e.CrossEntity)
                .WithMany(e => e.Attachments)
                .HasForeignKey(e => e.CrossEntityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}