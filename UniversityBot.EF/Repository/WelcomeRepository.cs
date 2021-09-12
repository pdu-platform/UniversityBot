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
    public class WelcomeRepository
    {
        public AppDbContext _context;

        public WelcomeRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<TRes>> SelectPageAsync<TRes>(int pageIndex, int pageSize,
            Expression<Func<BotWelcome, TRes>> map,
            Expression<Func<BotWelcome, bool>> filter = null, CancellationToken token = default)
        {
            return _context.Welcome.SelectPageAsync(pageIndex, pageSize, 
                query => query.OrderBy(e => e.Order).ThenBy(e => e.Text), 
                map, filter, token);
        }

        public void Delete(Guid welcomeId)
        {
            _context.RemoveByTrackInfo(_context.Welcome, BotWelcome.ForRemove(welcomeId));
        }

        public async Task AddAsync(BotWelcome welcome)
        {
            await _context.Welcome.AddAsync(welcome);
        }
        
        public void Update(BotWelcome welcome)
        {
            _context.Welcome.Update(welcome);
        }
    }
}