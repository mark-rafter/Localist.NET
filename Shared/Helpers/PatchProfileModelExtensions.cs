namespace Localist.Shared.Helpers
{
    public static class PatchProfileModelExtensions
    {
        public static PatchProfileModel WithBookmarkId(this PatchProfileModel model, string bookmarkId)
            => model with { BookmarkIds = new string[] { bookmarkId } };

        public static PatchProfileModel WithWatchId(this PatchProfileModel model, string watchId)
            => model with { WatchIds = new string[] { watchId } };
    }
}