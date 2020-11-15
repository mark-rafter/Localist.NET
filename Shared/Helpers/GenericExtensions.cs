using System.Collections.Generic;

namespace Localist.Shared.Helpers
{
    public static class GenericExtensions
    {
        public static HashSet<TItem> ToHashSet<TItem>(this TItem item)
            => new HashSet<TItem>(new[] { item });
    }
}