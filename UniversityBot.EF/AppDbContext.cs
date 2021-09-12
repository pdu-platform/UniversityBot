using Microsoft.EntityFrameworkCore;
using UniversityBot.Core.DAL;
using UniversityBot.Core.DAL.Attachment;

namespace UniversityBot.EF
{
    public class AppDbContext : DbContext
    {
        public DbSet<BotQuestioner> Questioner { get; private set; }
        public DbSet<BotWelcome> Welcome { get; private set; }
        public DbSet<BotCommand> Commands { get; private set; }
        public DbSet<BotMessageAttachment> MessageAttachments { get; private set; }
        public DbSet<BotCommandQuestion> CommandHandleNames { get; private set; }
        public DbSet<BotFile> Files { get; private set; }
        
        public DbSet<BotReactionKeyword> ReactionKeywords { get; private set; }
        
        public DbSet<BotSettings> Settings { get; private set; }
        
        public DbSet<NotFoundWord> NotFoundWords { get; private set; }
        
        public AppDbContext(DbContextOptions options) 
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}