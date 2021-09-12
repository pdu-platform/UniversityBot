using System.Buffers;

namespace UniversityBot.Blazor.Data
{
    public readonly struct PoolSharedData<T>
    {
        public readonly T[] Data;
        public readonly bool ClearArray;
        public readonly long RentSize;
        
        public PoolSharedData(T[] data, long rentSize, bool clearArray = false)
        {
            Data = data;
            RentSize = rentSize;
            ClearArray = clearArray;
        }
        
        public void Dispose()
        {
            if(Data == default)
                return;
            
            ArrayPool<T>.Shared.Return(Data, ClearArray);
        }
    }
}