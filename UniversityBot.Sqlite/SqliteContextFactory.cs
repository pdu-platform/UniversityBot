using Microsoft.EntityFrameworkCore;
using UniversityBot.EF;

namespace UniversityBot.Sqlite
{
    public class SqliteContextFactory : ContextFactoryBase
    {
        public SqliteContextFactory(string connectionString, bool ensureCreate) 
            : base(connectionString, ensureCreate)
        {
        }
        
        protected override void ConfigDatabaseConnection(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder.UseSqlite(connectionString);
        }

        public static string BuildConnectionString(string filePath) => $"Data Source={filePath}";
    }
}