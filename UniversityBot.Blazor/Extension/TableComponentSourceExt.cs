using UniversityBot.Blazor.Components.Table;

namespace UniversityBot.Blazor.Extension
{
    public static class TableComponentSourceExt
    {
        public static bool AnyCommandEnabled<T>(this ITableComponentSource<T> self)
            => self.EnableDeleteCommand || self.EnableEditCommand;
    }
}