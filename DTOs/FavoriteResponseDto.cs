using O_market.DTOs;

namespace O_market.DTO
{
    public class FavoriteResponseDto
    {
        public int Id { get; set; }

        public AdResponseDto Ad { get; set; } = null!;

        // Explicit image fields
        public string? PrimaryImageUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
