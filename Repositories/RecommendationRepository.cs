using Microsoft.EntityFrameworkCore;
using O_market.Interfaces.Repositories;
using O_market.Models;

namespace O_market.Repositories
{
    public class RecommendationRepository : IRecommendationRepository
    {
        private readonly OlxdbContext _context;

        public RecommendationRepository(OlxdbContext context)
        {
            _context = context;
        }

        public async Task<List<Ad>> GetRecommendedAdsAsync(
            int userId,
            List<int> preferredCategoryIds,
            decimal? avgPrice,
            int take)
        {
            var query = _context.Ads
                .Include(a => a.Category)
                .Include(a => a.AdImages)
                .Where(a =>
                    a.UserId != userId &&
                    a.Status == "Active");

            if (preferredCategoryIds.Any())
            {
                query = query.Where(a => preferredCategoryIds.Contains(a.CategoryId));
            }

            if (avgPrice.HasValue)
            {
                query = query.Where(a =>
                    a.Price >= avgPrice.Value * 0.7m &&
                    a.Price <= avgPrice.Value * 1.3m);
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Take(take)
                .ToListAsync();
        }
    }
}
