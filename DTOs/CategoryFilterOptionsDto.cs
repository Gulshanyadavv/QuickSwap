using O_market.DTO;

public class CategoryFilterOptionsDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int? ParentCategoryId { get; set; }
    public List<CategoryResponseDto> SubCategories { get; set; } = new();
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? AvgPrice { get; set; }
    public List<string> PopularLocations { get; set; } = new();
    public Dictionary<string, List<string>> FieldOptions { get; set; } = new();
    public int TotalAds { get; set; }
    public DateTime? OldestAdDate { get; set; }
    public DateTime? NewestAdDate { get; set; }
}