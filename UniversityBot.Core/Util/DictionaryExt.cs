using System;
using System.Collections.Generic;

namespace UniversityBot.Core.Util
{
    public static class DictionaryExt
    {
        public static TV GetOrAdd<TK, TV>(this Dictionary<TK, TV> self, TK key, Func<TK, TV> factory)
        {
            if (self.TryGetValue(key, out var v))
                return v;

            v = factory(key);
            self.Add(key, v);
            return v;
        }
    }
}