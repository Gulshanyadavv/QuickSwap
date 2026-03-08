using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using O_market.DTO;
using O_market.DTOs;
using O_market.Models;
using O_market.Repositories;

namespace O_market.Repositories
{
    public class AdRepository : IAdRepository
    {
        private readonly OlxdbContext _context;
        private readonly ILogger<AdRepository> _logger;

        public AdRepository(OlxdbContext context, ILogger<AdRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Ad?> GetByIdAsync(int id, int? userId = null)
        {
            var query = _context.Ads
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.AdImages.OrderBy(i => i.DisplayOrder))
                .Include(a => a.AdDynamicValues)
                    .ThenInclude(dv => dv.Field)
                .Include(a => a.Favorites)
                .Where(a => a.Id == id);

            if (userId.HasValue)
            {
                query = query.Where(a => a.UserId == userId.Value || a.Status == "Active");
            }
            else
            {
                query = query.Where(a => a.Status == "Active");
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<PagedList<Ad>> SearchAdsAsync(OlxSearchDto searchDto)
        {
            var query = _context.Ads
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.AdImages)
                .Include(a => a.Favorites)
                .Where(a => a.Status == "Active")
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchDto.Search))
                query = query.Where(a =>
                    a.Title.Contains(searchDto.Search) ||
                    a.Description.Contains(searchDto.Search));

            if (searchDto.CategoryId.HasValue)
                query = query.Where(a => a.CategoryId == searchDto.CategoryId);

            if (!string.IsNullOrEmpty(searchDto.Location))
                query = query.Where(a => a.Location.Contains(searchDto.Location));

            if (searchDto.MinPrice.HasValue)
                query = query.Where(a => a.Price >= searchDto.MinPrice);

            if (searchDto.MaxPrice.HasValue)
                query = query.Where(a => a.Price <= searchDto.MaxPrice);

            // GEOLOCATION: Efficient Bounding Box Filter
            // This filters ads roughly within the radius before doing heavy math in-memory
            if (searchDto.Latitude.HasValue && searchDto.Longitude.HasValue)
            {
                double latRange = searchDto.RadiusInKm / 111.0; // 1 degree lat is ~111km
                double lonRange = searchDto.RadiusInKm / (111.0 * Math.Cos(searchDto.Latitude.Value * (Math.PI / 180.0)));

                double minLat = searchDto.Latitude.Value - latRange;
                double maxLat = searchDto.Latitude.Value + latRange;
                double minLon = searchDto.Longitude.Value - lonRange;
                double maxLon = searchDto.Longitude.Value + lonRange;

                query = query.Where(a => a.Latitude >= minLat && a.Latitude <= maxLat &&
                                        a.Longitude >= minLon && a.Longitude <= maxLon);
            }

            var total = await query.CountAsync();

            var ads = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // Refine with exact Haversine distance and sorting
            if (searchDto.Latitude.HasValue && searchDto.Longitude.HasValue)
            {
                ads = ads.Select(a => {
                    if (a.Latitude.HasValue && a.Longitude.HasValue)
                    {
                        // Haversine calculation
                        double dLat = (a.Latitude.Value - searchDto.Latitude.Value) * (Math.PI / 180.0);
                        double dLon = (a.Longitude.Value - searchDto.Longitude.Value) * (Math.PI / 180.0);
                        double lat1 = (searchDto.Latitude.Value) * (Math.PI / 180.0);
                        double lat2 = (a.Latitude.Value) * (Math.PI / 180.0);

                        double val = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                                     Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
                        double c = 2 * Math.Atan2(Math.Sqrt(val), Math.Sqrt(1 - val));
                        double distance = 6371 * c; // Earth radius in km

                        // Use a temporary field or Tag to pass distance if needed
                        // For now we'll just sort by it
                    }
                    return a;
                })
                .OrderBy(a => GetDistance(searchDto.Latitude.Value, searchDto.Longitude.Value, a.Latitude, a.Longitude))
                .ToList();
            }

            var pagedAds = ads
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToList();

            return new PagedList<Ad>(pagedAds, total, searchDto.Page, searchDto.PageSize);
        }

        private double GetDistance(double lat1, double lon1, double? lat2, double? lon2)
        {
            if (!lat2.HasValue || !lon2.HasValue) return double.MaxValue;

            double dLat = (lat2.Value - lat1) * (Math.PI / 180.0);
            double dLon = (lon2.Value - lon1) * (Math.PI / 180.0);
            double rLat1 = (lat1) * (Math.PI / 180.0);
            double rLat2 = (lat2.Value) * (Math.PI / 180.0);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(rLat1) * Math.Cos(rLat2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return 6371 * c;
        }

        public async Task<Ad> CreateAsync(Ad ad)
        {
            _context.Ads.Add(ad);
            await _context.SaveChangesAsync();
            return ad;
        }

        public async Task<Ad> UpdateAsync(Ad ad)
        {
            _context.Ads.Update(ad);
            await _context.SaveChangesAsync();
            return ad;
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var ad = await _context.Ads
                    .Include(a => a.AdImages)
                    .Include(a => a.AdDynamicValues)
                    .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

                if (ad == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // Remove ad references from messages
                var messages = await _context.Messages
                    .Where(m => m.AdId == id)
                    .ToListAsync();

                if (messages.Any())
                {
                    foreach (var message in messages)
                    {
                        message.AdId = null;
                    }
                    await _context.SaveChangesAsync();
                }

                // Delete activity records (recommendations)
                var activities = await _context.UserAdActivities
                    .Where(ua => ua.AdId == id)
                    .ToListAsync();

                if (activities.Any())
                {
                    _context.UserAdActivities.RemoveRange(activities);
                    await _context.SaveChangesAsync();
                }

                // Delete favorites
                var favorites = await _context.Favorites
                    .Where(f => f.AdId == id)
                    .ToListAsync();

                if (favorites.Any())
                {
                    _context.Favorites.RemoveRange(favorites);
                    await _context.SaveChangesAsync();
                }

                // Delete dynamic values
                if (ad.AdDynamicValues.Any())
                {
                    _context.AdDynamicValues.RemoveRange(ad.AdDynamicValues);
                    await _context.SaveChangesAsync();
                }

                // Delete images
                if (ad.AdImages.Any())
                {
                    _context.AdImages.RemoveRange(ad.AdImages);
                    await _context.SaveChangesAsync();
                }

                // Finally delete the ad
                _context.Ads.Remove(ad);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation("Deleted ad {AdId} by user {UserId}", id, userId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Delete failed for ad {AdId} by user {UserId}", id, userId);
                throw;
            }
        }

        public async Task AddImagesAsync(int adId, List<AdImage> images)
        {
            foreach (var img in images)
                img.AdId = adId;

            _context.AdImages.AddRange(images);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AdImage>> GetImagesAsync(int adId)
        {
            return await _context.AdImages
                .Where(i => i.AdId == adId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();
        }

        public async Task DeleteImagesAsync(int adId)
        {
            var images = await GetImagesAsync(adId);
            _context.AdImages.RemoveRange(images);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsOwnedByUserAsync(int adId, int userId)
        {
            return await _context.Ads.AnyAsync(a => a.Id == adId && a.UserId == userId);
        }

        public async Task<PagedList<Ad>> GetUserAdsAsync(int userId, int page, int pageSize)
        {
            var query = _context.Ads
                .Include(a => a.Category)
                .Include(a => a.AdImages)
                .Include(a => a.Favorites)
                .Where(a => a.UserId == userId)
                .AsQueryable();

            var total = await query.CountAsync();

            var ads = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<Ad>(ads, total, page, pageSize);
        }

        public async Task ToggleFavoriteAsync(int adId, int userId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.AdId == adId && f.UserId == userId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
            }
            else
            {
                _context.Favorites.Add(new Favorite
                {
                    AdId = adId,
                    UserId = userId
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task AddDynamicValuesAsync(int adId, List<AdDynamicValue> dynamicValues)
        {
            foreach (var dv in dynamicValues)
            {
                dv.AdId = adId;
                dv.CreatedAt = DateTime.UtcNow;
            }

            _context.AdDynamicValues.AddRange(dynamicValues);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AdDynamicValue>> GetDynamicValuesAsync(int adId)
        {
            return await _context.AdDynamicValues
                .Include(dv => dv.Field)
                .Where(dv => dv.AdId == adId)
                .ToListAsync();
        }
    }
}