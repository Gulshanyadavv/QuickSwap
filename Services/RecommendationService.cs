using Microsoft.EntityFrameworkCore;
using O_market.DTOs.Recommendation;
using O_market.Interfaces.Repositories;
using O_market.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using O_market.Models;

namespace O_market.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly OlxdbContext _context;
        private readonly IRecommendationRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RecommendationService(
            OlxdbContext context,
            IRecommendationRepository repository,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetFullImageUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return "/assets/no-image.png";

            if (relativePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return relativePath;

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return relativePath;

            return $"{request.Scheme}://{request.Host}{relativePath}";
        }

        public async Task<RecommendationResponseDto> GetRecommendationsAsync(
            int userId,
            int take = 10)
        {
            // 1️⃣ User activity
            var activities = await _context.UserAdActivities
                .Where(x => x.UserId == userId)
                .Include(x => x.Ad)
                .ToListAsync();

            var preferredCategories = activities
                .Select(x => x.Ad.CategoryId)
                .Distinct()
                .ToList();

            var avgPrice = activities
                .Where(x => x.Ad.Price > 0)
                .Select(x => x.Ad.Price)
                .DefaultIfEmpty()
                .Average();

            // 2️⃣ Fetch recommended ads
            var ads = await _repository.GetRecommendedAdsAsync(
                userId,
                preferredCategories,
                avgPrice,
                take);

            // 3️⃣ Favorites
            var favoriteAdIds = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.AdId)
                .ToListAsync();

            var result = ads.Select(a => new RecommendedAdDto
            {
                Id = a.Id,
                Title = a.Title,
                Price = a.Price,
                Location = a.Location,
                CategoryName = a.Category.Name,
                PrimaryImageUrl = GetFullImageUrl(a.AdImages
                    .FirstOrDefault(i => i.IsPrimary == true)?.ImageUrl),
                IsFavorited = favoriteAdIds.Contains(a.Id),
                Latitude = a.Latitude,
                Longitude = a.Longitude
            }).ToList();

            return new RecommendationResponseDto
            {
                Total = result.Count,
                Items = result
            };
        }
    }
}
