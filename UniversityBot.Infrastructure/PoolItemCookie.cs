using System;
using Microsoft.Extensions.ObjectPool;

namespace UniversityBot.Infrastructure
{
    public readonly struct PoolItemCookie<T> : IDisposable
        where T : class
    {
        private readonly ObjectPool<T> _pool;
        
        public T Item { get; }
        
        public PoolItemCookie(ObjectPool<T> pool)
        {
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            Item = pool.Get();
        }

        public static implicit operator T(PoolItemCookie<T> cookie) => cookie._pool != null ?
            cookie.Item : throw new ArgumentNullException();
        
        public void Dispose()
        {
            _pool?.Return(Item);
        }
    }
}