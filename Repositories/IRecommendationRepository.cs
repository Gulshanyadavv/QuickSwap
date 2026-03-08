using O_market.Models;

namespace O_market.Interfaces.Repositories
{
    public interface IRecommendationRepository
    {
        Task<List<Ad>> GetRecommendedAdsAsync(
            int userId,
            List<int> preferredCategoryIds,
            decimal? avgPrice,
            int take);
    }
}
