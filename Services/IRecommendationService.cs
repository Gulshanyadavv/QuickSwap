using O_market.DTOs.Recommendation;

namespace O_market.Interfaces.Services
{
    public interface IRecommendationService
    {
        Task<RecommendationResponseDto> GetRecommendationsAsync(
            int userId,
            int take = 10);
    }
}
