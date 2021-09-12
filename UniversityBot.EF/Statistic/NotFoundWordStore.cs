using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniversityBot.Core.DAL;
using UniversityBot.Core.Statistic;

namespace UniversityBot.EF.Statistic
{
    public sealed class NotFoundWordStore : INotFoundWordStore
    {
        private readonly AppDbContext _ctx;

        public NotFoundWordStore(AppDbContext ctx)
        {
            _ctx = ctx;
        }
        
        public async Task Save(NotFoundStoreEntity notFoundStoreEntity)
        {
            var insertEntity = AsNotFoundWord(notFoundStoreEntity);
            await _ctx.AddAsync(insertEntity);
            await SaveCore();
        }

        private async Task SaveCore()
        {
            await _ctx.SaveChangesAsync();
            
            foreach (var entityEntry in _ctx.ChangeTracker.Entries<NotFoundWord>())
                entityEntry.State = EntityState.Detached;
        }
        
        private static NotFoundWord AsNotFoundWord(in NotFoundStoreEntity notFoundStoreEntity)
        {
            return new NotFoundWord(Guid.Empty, notFoundStoreEntity.Word, DateTime.UtcNow);
        }
    }
}