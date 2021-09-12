using System.Collections.Generic;

namespace UniversityBot.Core.Util
{
    public static class EnumerableExt
    {
        public static T[] ToArray<T>(this IEnumerable<T> self, int count)
        {
            var res = new T[count];
            var idx = 0;
            foreach (var t in self)
            {
                res[idx] = t;
                idx += 1;
            }

            return res;
        }
    }
}