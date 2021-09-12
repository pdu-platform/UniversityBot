using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityBot.Core.DAL;

namespace UniversityBot.EF.Configuration
{
    public class BotReactionKeywordConfiguration : BotEntityConfigurationBase<BotReactionKeyword>
    {
        protected override string EntityName => "tbx_Bot_Reaction_Keyword";
        
        protected override void ConfigureEntity(EntityTypeBuilder<BotReactionKeyword> builder)
        {
            builder
                .Property(e => e.Word)
                .HasColumnName("word")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(e => e.Word);

            builder
                .Property(e => e.Hash)
                .HasColumnName("hash")
                .IsRequired();

            builder.HasIndex(e => e.Hash);
            
            builder
                .Property(e => e.Required)
                .HasColumnName("required")
                .IsRequired();
            
            builder
                .Property(e => e.QuestionerId)
                .HasColumnName("id_questioner");

            builder
                .HasOne(e => e.Questioner)
                .WithMany(e => e.ReactionKeywords)
                .HasForeignKey(e => e.QuestionerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}