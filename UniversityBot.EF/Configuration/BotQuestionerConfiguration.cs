using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityBot.Core.DAL;

namespace UniversityBot.EF.Configuration
{
    public class BotQuestionerConfiguration : BotEntityConfigurationBase<BotQuestioner>
    {
        protected override string EntityName => "tbx_Bot_Questioner";
        
        protected override void ConfigureEntity(EntityTypeBuilder<BotQuestioner> builder)
        {
            builder
                .Property(e => e.Question)
                .HasColumnName("question")
                .IsRequired();
            
            builder
                .Property(e => e.Answer)
                .HasColumnName("answer");
            
            builder
                .Property(e => e.ParentId)
                .HasColumnName("id_parent");
            
            builder
                .HasOne(e => e.Parent)
                .WithMany()
                .HasForeignKey(e => e.ParentId);
        }
    }
}