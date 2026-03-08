using System.ComponentModel.DataAnnotations;

namespace O_market.DTO
{
    public class AdSearchEnhancedDto : AdSearchDto
    {
        // Additional OLX-like filters
        public List<int>? CategoryIds { get; set; }
        public bool? IncludeSubcategories { get; set; } = true;
        public string? SortBy { get; set; } = "newest";
        public bool? VerifiedSellerOnly { get; set; }
        public DateTime? PostedAfter { get; set; }

        // Dynamic field filters
        public Dictionary<string, string>? DynamicFilters { get; set; }

        // Pagination (inherited from AdSearchDto)
        // Search (inherited from AdSearchDto)
        // Location (inherited from AdSearchDto)
        // Price (inherited from AdSearchDto)
    }
}