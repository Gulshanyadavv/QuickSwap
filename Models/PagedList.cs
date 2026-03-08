using System.Collections.Generic;

namespace O_market.Models  // Matches your project's namespace
{
    /// <summary>
    /// A generic helper for paginated results (e.g., from DB queries).
    /// </summary>
    /// <typeparam name="T">The type of items (e.g., Ad).</typeparam>
    public class PagedList<T>
    {
        public List<T> Items { get; }
        public int TotalCount { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);  // Optional: Computed property for total pages

        public PagedList(List<T> items, int total, int page, int pageSize)
        {
            Items = items ?? new List<T>();  // Null-safe
            TotalCount = total;
            Page = page < 1 ? 1 : page;  // Ensure min page 1
            PageSize = pageSize < 1 ? 10 : pageSize;  // Default min 10
        }
    }
}