namespace O_market.DTOs.Recommendation
{
    public class RecommendedAdDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public decimal Price { get; set; }
        public string Location { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string? PrimaryImageUrl { get; set; }
        public bool IsFavorited { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? DistanceKm { get; set; }
    }
}
