using System.Collections.Generic;

namespace O_market.DTOs.Recommendation
{
    public class RecommendationResponseDto
    {
        public int Total { get; set; }
        public List<RecommendedAdDto> Items { get; set; } = new();
    }
}
