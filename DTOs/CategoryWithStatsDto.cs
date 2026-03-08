using O_market.DTO;

public class CategoryWithStatsDto : CategoryResponseDto
{
    public int AdsCount { get; set; }
    public string? Icon { get; set; }
    public bool IsPopular { get; set; }
    public int TodayAds { get; set; }
}