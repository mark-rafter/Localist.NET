using System.Collections.Generic;

namespace Localist.Shared
{
    public record TreeItem<TItem>(TItem Item, List<TreeItem<TItem>> Children);
}