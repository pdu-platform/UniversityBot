using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace UniversityBot.EF.Extension
{
    public static class QueryableExt
    {
        private static IQueryable<T> Page<T>(this IQueryable<T> self, int page, int pageSize)
        {
            return self
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }
        
        public static Task<List<TRes>> SelectPageAsync<TModel, TRes>(this IQueryable<TModel> self, 
            int page, int pageSize,
            Func<IQueryable<TModel>, IOrderedQueryable<TModel>> mapToOrderQueryable,
            Expression<Func<TModel, TRes>> map,
            Expression<Func<TModel, bool>> filter = null, CancellationToken token = default) 
            where TModel : class
        {
            var query = self.AsNoTracking();
            if (filter != null)
                query = query.Where(filter);
            
            return mapToOrderQueryable(query)
                .Page(page, pageSize)
                .Select(map)
                .ToListAsync(cancellationToken: token);
        }
    }
}