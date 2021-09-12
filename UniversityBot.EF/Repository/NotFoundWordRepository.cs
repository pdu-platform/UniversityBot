using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UniversityBot.Core.DAL;
using UniversityBot.EF.Extension;

namespace UniversityBot.EF.Repository
{
    public class NotFoundWordRepository
    {
        private AppDbContext _db;

        public NotFoundWordRepository(AppDbContext db)
        {
            _db = db;
        }
        
        public Task<List<TRes>> SelectPageAsync<TRes>(int pageIndex, int pageSize,
            Expression<Func<NotFoundWord, TRes>> map,
            Expression<Func<NotFoundWord, bool>> filter = null, CancellationToken token = default)
        {
            return _db.NotFoundWords
                .SelectPageAsync(pageIndex, pageSize, 
                    words => Queryable.OrderByDescending(words, e => e.CreateTime), 
                    map, filter, token);
        }
    }
}