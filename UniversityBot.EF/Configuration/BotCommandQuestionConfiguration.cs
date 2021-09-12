using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityBot.Core.DAL;

namespace UniversityBot.EF.Configuration
{
    public class BotCommandQuestionConfiguration : BotEntityConfigurationBase<BotCommandQuestion>
    {
        protected override string EntityName => "tbx_Bot_Command_Question";
        protected override void ConfigureEntity(EntityTypeBuilder<BotCommandQuestion> builder)
        {
            builder
                .Property(e => e.Question)
                .HasMaxLength(512)
                .HasColumnName("question")
                .IsRequired();

            builder
                .HasIndex(e => e.Question)
                .IsUnique();
            
            builder
                .HasOne(e => e.BotCommand)
                .WithMany(e => e.Questions)
                .HasForeignKey(e => e.BotCommandId);

            builder
                .Property(e => e.BotCommandId)
                .HasColumnName("id_bot_command")
                .IsRequired();
        }
    }
}