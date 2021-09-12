using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityBot.Core.DAL;

namespace UniversityBot.EF.Configuration
{
    public sealed class BotFileConfiguration : BotEntityConfigurationBase<BotFile>
    {
        protected override string EntityName => "tbx_File";
        
        protected override void ConfigureEntity(EntityTypeBuilder<BotFile> builder)
        {
            builder
                .Property(e => e.Content)
                .IsRequired()
                .HasColumnName("content");
            
            builder
                .Property(e => e.Extension)
                .IsRequired()
                .HasMaxLength(16)
                .HasColumnName("extension");
            
            builder
                .Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name");
            
            builder
                .Property(e => e.Size)
                .IsRequired()
                .HasColumnName("size");
        }
    }
}