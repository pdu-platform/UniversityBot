using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityBot.Core.DAL;
using UniversityBot.Core.DAL.Attachment;

namespace UniversityBot.EF.Configuration
{
    public abstract class BotAttachmentConfiguration<T, TCross> : BotEntityConfigurationBase<T>
        where T : BotAttachment<T, TCross> 
        where TCross : BotEntity
    {
        protected abstract override string EntityName { get; }
        
        protected override void ConfigureEntity(EntityTypeBuilder<T> builder)
        {
            builder
                .Property(e => e.FileId)
                .HasColumnName("id_file")
                .IsRequired();
            
            builder
                .HasOne(e => e.File)
                .WithMany()
                .HasForeignKey(e => e.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Property(e => e.CrossEntityId)
                .HasColumnName("id_cross_entity")
                .IsRequired();
        }
    }
}