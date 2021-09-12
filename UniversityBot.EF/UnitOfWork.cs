using System;
using System.Threading.Tasks;
using Dawn;
using Microsoft.EntityFrameworkCore.Storage;
using UniversityBot.EF.Repository;

namespace UniversityBot.EF
{
    public sealed class UnitOfWork
    {
        public readonly AppDbContext Database;

        public QuestionRepository QuestionRepository { get; }

        public AttachmentRepository AttachmentRepository { get; }
        
        public CommandRepository CommandRepository { get; }
        
        public ReactionKeywordRepository ReactionKeywordRepository { get; }
        
        public WelcomeRepository WelcomeRepository { get; }
        
        public NotFoundWordRepository NotFoundWordRepository { get; }
        
        public UnitOfWork(AppDbContext db)
        {
            Database = Guard.Argument(db, nameof(db)).NotNull();

            WelcomeRepository = new WelcomeRepository(db);
            QuestionRepository = new QuestionRepository(db);
            AttachmentRepository = new AttachmentRepository(db);
            CommandRepository = new CommandRepository(db);
            ReactionKeywordRepository = new ReactionKeywordRepository(db);
            NotFoundWordRepository = new NotFoundWordRepository(db);
        }


        public Task<int> SaveChangesAsync() => Database.SaveChangesAsync();
        
        public int SaveChanges() => Database.SaveChanges();

        public AsyncSaveCookie AsyncSaveScope() => new(Database);
        public SaveCookie SaveScope() => new(Database);

        public async Task<AsyncTransactScope> TransactScopeAsync()
        {
            var scope = await Database.Database.BeginTransactionAsync();
            return new AsyncTransactScope(scope);
        }
        
        public readonly ref struct SaveCookie
        {
            private readonly AppDbContext _content;

            public SaveCookie(AppDbContext content)
            {
                _content = content;
            }

            public void Dispose() => _content.SaveChanges();
        }
        
        public readonly struct AsyncSaveCookie : IAsyncDisposable
        {
            private readonly AppDbContext _context;

            public AsyncSaveCookie(AppDbContext context)
            {
                _context = context;
            }

            public ValueTask DisposeAsync() => new(_context.SaveChangesAsync());
        }

        public readonly struct AsyncTransactScope
        {
            private readonly IDbContextTransaction _tran;

            public AsyncTransactScope(IDbContextTransaction tran)
            {
                _tran = tran;
            }

            public ValueTask DisposeAsync()
            {
                return _tran.DisposeAsync();
            }
        }
    }
}