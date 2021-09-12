using System;
using System.Collections.Generic;
using Dawn;

namespace UniversityBot.Core.Util
{
    public static class ListExt
    {
        public static List<T> ToList<T>(this IEnumerable<T> self, int capacity)
        {
            var res = new List<T>(capacity);
            res.AddRange(self);
            return res;
        }
    }
}