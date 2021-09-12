using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityBot.Core.DAL;

namespace UniversityBot.EF.Configuration
{
    public class BotWelcomeConfiguration : BotEntityConfigurationBase<BotWelcome>
    {
        protected override string EntityName => "tbx_Bot_Welcome";
        
        protected override void ConfigureEntity(EntityTypeBuilder<BotWelcome> builder)
        {
            builder
                .Property(e => e.Text)
                .HasColumnName("text")
                .IsRequired();

            builder
                .Property(e => e.Order)
                .HasColumnName("order")
                .IsRequired();
        }
    }
}