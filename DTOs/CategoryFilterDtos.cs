namespace O_market.DTOs
{
    // For sidebar filters like OLX
    public class CategoryFilterSidebarDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public PriceFilterDto PriceFilter { get; set; } = new();
        public LocationFilterDto LocationFilter { get; set; } = new();
        public List<DynamicFieldFilterDto> DynamicFilters { get; set; } = new();
        public List<FilterOptionDto> ConditionFilters { get; set; } = new();
        public List<FilterOptionDto> SellerTypeFilters { get; set; } = new();
        public List<FilterOptionDto> PostedDateFilters { get; set; } = new();
    }

    public class PriceFilterDto
    {
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public List<PriceRangeOption> QuickRanges { get; set; } = new();
    }

    public class PriceRangeOption
    {
        public string Label { get; set; } = null!;
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
    }

    public class LocationFilterDto
    {
        public List<string> PopularCities { get; set; } = new();
        public List<string> AllLocations { get; set; } = new();
    }

    public class DynamicFieldFilterDto
    {
        public int FieldId { get; set; }
        public string Label { get; set; } = null!;
        public string FieldType { get; set; } = null!;
        public List<FilterOptionDto> Options { get; set; } = new();
        public bool IsMultiSelect { get; set; }
    }

    public class FilterOptionDto
    {
        public string Value { get; set; } = null!;
        public string Label { get; set; } = null!;
        public int Count { get; set; }
        public bool IsSelected { get; set; }
    }
}