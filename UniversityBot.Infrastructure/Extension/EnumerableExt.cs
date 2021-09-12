using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dawn;

namespace UniversityBot.Infrastructure.Extension
{
    public static class EnumerableExt
    {
        private static int? TryGetCountSafe<T>(this IEnumerable<T> self)
        {
            return self switch
            {
                ICollection c => c.Count,
                ICollection<T> l => l.Count,
                IReadOnlyCollection<T> rl => rl.Count,
                string str => str.Length,
                _ => null
            };
        }
        
        public static IEnumerable<T> AllDuplicates<T>(this IEnumerable<T> self, IEqualityComparer<T> comparer = null)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            var count = self.TryGetCountSafe();
            
            var store = count.HasValue ? 
                new Dictionary<T, int>(count.Value, comparer) : 
                new Dictionary<T, int>(comparer);
            
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var item in self)
            {
                if (!store.TryGetValue(item, out var val))
                {
                    store.Add(item, 1);
                }
                else
                {
                    store[item] = val + 1;
                }
            }

            return store
                .Where(e => e.Value > 1)
                .Select(e => e.Key);
        }
        
        public static string JoinToString<T>(this IEnumerable<T> collect, 
            string separator = ", ",
            Func<T, string> valueConverter = null,
            string prefix = null,
            string postfix = null,
            int? restrictionCount = null,
            string restrictionStr = "...")
        {
            Guard.Argument(collect, nameof(collect)).NotNull();
            
            return new StringBuilder(256)
                .JointToString(collect, valueConverter, separator, prefix, postfix, restrictionCount, restrictionStr)
                .ToString();
        }
    }
}