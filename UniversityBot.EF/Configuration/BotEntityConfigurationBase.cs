using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityBot.Core.DAL;

namespace UniversityBot.EF.Configuration
{
    public abstract class BotEntityConfigurationBase<T> : IEntityTypeConfiguration<T> where T : BotEntity
    {
        protected abstract string EntityName { get; }
        
        public void Configure(EntityTypeBuilder<T> builder)
        {
            builder.ToTable(EntityName);
            
            builder.HasKey(e => e.Id);
            
            builder
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            
            ConfigureEntity(builder);
        }

        protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);
    }
}