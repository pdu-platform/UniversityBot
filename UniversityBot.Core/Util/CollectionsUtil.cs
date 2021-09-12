using System;
using System.Collections.Generic;
using System.Linq;

namespace UniversityBot.Core.Util
{
    public static class CollectionsUtil
    {
        public static bool RemoveFirst<T>(this List<T> self, Predicate<T> searchKey)
        {
            var idx = self.FindIndex(searchKey);
            if (idx < 0)
                return false;
            
            self.RemoveAt(idx);
            return true;
        }
        
        public static bool AreSame<T>(ICollection<T> c1, ICollection<T> c2, IEqualityComparer<T> comparer = null)
        {
            if (c1 == null && c2 == null || ReferenceEquals(c1, c2))
                return true;
            if (c1 == null)
                return false;
            if (c2 == null)
                return false;
            if (c1.Count != c2.Count)
                return false;
            
            return comparer != null ? 
                c1.SequenceEqual(c2, comparer) : 
                c1.SequenceEqual(c2);
        }

        public static int HashCode<T>(ICollection<T> self, IEqualityComparer<T> comparer = null)
        {
            if (self == null || self.Count == 0)
                return 92;
            
            comparer ??= EqualityComparer<T>.Default;
            return self.Aggregate(1, (i, arg2) => System.HashCode.Combine(i, comparer.GetHashCode(arg2)));
        }
    }
}
