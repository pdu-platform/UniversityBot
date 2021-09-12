using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityBot.Core.DAL;

namespace UniversityBot.EF.Configuration
{
    public class NotFoundWordConfiguration : BotEntityConfigurationBase<NotFoundWord>
    {
        protected override string EntityName => "tbx_Not_Found_Word";
        
        protected override void ConfigureEntity(EntityTypeBuilder<NotFoundWord> builder)
        {
            builder.Property(e => e.Question)
                .HasColumnName("question")
                .IsRequired();

            builder.Property(e => e.CreateTime)
                .HasColumnName("create_time")
                .IsRequired();
        }
    }
}