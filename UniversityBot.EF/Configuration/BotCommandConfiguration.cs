using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityBot.Core.DAL;

namespace UniversityBot.EF.Configuration
{
    public class BotCommandConfiguration : BotEntityConfigurationBase<BotCommand>
    {
        protected override string EntityName => "tbx_Bot_Command";
        protected override void ConfigureEntity(EntityTypeBuilder<BotCommand> builder)
        {
            builder
                .Property(e => e.Answer)
                .HasColumnName("answer")
                .IsRequired();

            builder
                .Property(e => e.UserFriendlyName)
                .HasColumnName("user_friendly_name")
                .HasMaxLength(512);

            builder
                .Property(e => e.ShowInAllCommandList)
                .HasColumnName("show_in_all_command_list")
                .IsRequired();
        }
    }
}