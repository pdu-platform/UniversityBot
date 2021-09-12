using Microsoft.Extensions.ObjectPool;

namespace UniversityBot.Infrastructure.Extension
{
    public static class ObjectPoolExt
    {
        public static PoolItemCookie<T> GetScoped<T>(this ObjectPool<T> self)
            where T : class => new(self);
    }
}