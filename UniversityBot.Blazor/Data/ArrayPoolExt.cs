using System;
using System.Buffers;

namespace UniversityBot.Blazor.Data
{
    public static class ArrayPoolExt
    {
        public static PoolSharedData<T> RentAsPoolSharedData<T>(this ArrayPool<T> self, long size, bool clearArray = false)
        {
            if (size > int.MaxValue)
                throw new ArgumentException(nameof(size));
            
            var array = self.Rent((int)size);
            return new PoolSharedData<T>(array, size, clearArray);
        }
    }
}