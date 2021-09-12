using Dawn;
using Microsoft.EntityFrameworkCore;

namespace UniversityBot.EF
{
    public abstract class ContextFactoryBase : IContextFactory
    {
        private readonly string _connectionString;
        private readonly bool _ensureCreate;

        public ContextFactoryBase(string connectionString, bool ensureCreate)
        {
            _connectionString = Guard.Argument(connectionString, nameof(connectionString)).NotNull();
            _ensureCreate = ensureCreate;
        }
        
        public AppDbContext Create(DbContextOptionsBuilder optionsBuilder)
        {
            ConfigDatabaseConnection(optionsBuilder, _connectionString);
            
            var db = new AppDbContext(optionsBuilder.Options);
            
            if (_ensureCreate)
                db.Database.EnsureCreated();
            
            return db;
        }

        protected abstract void ConfigDatabaseConnection(DbContextOptionsBuilder optionsBuilder, string connectionString);
    }
}