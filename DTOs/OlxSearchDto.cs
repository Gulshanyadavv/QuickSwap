namespace O_market.DTO
{
    public class OlxSearchDto
    {
        // Search box
        public string? Search { get; set; }

        // Category
        public int? CategoryId { get; set; }
        public bool IncludeSubcategories { get; set; } = true;

        // Location
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double RadiusInKm { get; set; } = 10; // Default 10km

        // Price filter
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Dynamic field filters
        // Example: { "Brand": "Honda", "Fuel": "Petrol" }
        public Dictionary<string, string>? DynamicFilters { get; set; }

        // Sorting
        // newest | price_low | price_high
        public string SortBy { get; set; } = "newest";

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
