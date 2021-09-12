using System.Collections.Generic;
using System.Linq;
using Mapster;

namespace UniversityBot.Blazor.Extension
{
    public static class ConverterFactory
    {
        public static To<TRes> To<TRes>() => new();
    }
    
    public struct To<T>
    {
    }
    
    public static class MapperExt
    {
        public static T Clone<T>(this T self) => self.Map(ConverterFactory.To<T>());
        public static TRes Map<T, TRes>(this T self, To<TRes> t) => self.Adapt<T, TRes>();
        public static IQueryable<TRes> Project<T, TRes>(this IQueryable<T> self, To<TRes> t) => self.ProjectToType<TRes>();
        public static IEnumerable<TRes> Project<T, TRes>(this IEnumerable<T> self, To<TRes> t) => self.Adapt<IEnumerable<TRes>>();
    }
}