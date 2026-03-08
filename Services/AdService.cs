using AutoMapper;
using Microsoft.EntityFrameworkCore;
using O_market.DTO;
using O_market.DTOs;
using O_market.Models;
using O_market.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace O_market.Services
{
    public class AdService : IAdService
    {
        private readonly OlxdbContext _context;
        private readonly IAdRepository _adRepo;
        private readonly IFavoriteRepository _favoriteRepo;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdService(
            OlxdbContext context,
            IAdRepository adRepo,
            IFavoriteRepository favoriteRepo,
            IMapper mapper,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _adRepo = adRepo;
            _favoriteRepo = favoriteRepo;
            _mapper = mapper;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper method to construct full image URLs
        private string GetFullImageUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return "/assets/no-image.png";

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return relativePath;

            return $"{request.Scheme}://{request.Host}{relativePath}";
        }

        // =====================================================
        // 1️⃣ BROWSE ADS (HOME FEED) - KEEP FOR COMPATIBILITY
        // =====================================================
        public async Task<PagedList<AdResponseDto>> GetLatestAdsAsync(
            int page,
            int pageSize,
            int? currentUserId)
        {
            // For backward compatibility, call SearchAdsAsync with default parameters
            var searchDto = new OlxSearchDto
            {
                Page = page,
                PageSize = pageSize,
                SortBy = "newest"
            };

            return await SearchAdsAsync(searchDto, currentUserId);
        }

        // =====================================================
        // 2️⃣ VIEW AD (DETAIL PAGE + AI TRACKING)
        // =====================================================
        public async Task<AdResponseWithDynamicDto?> GetAdDetailsAsync(
            int adId,
            int? currentUserId,
            bool skipTracking = false)
        {
            var ad = await _context.Ads
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.AdImages)
                .Include(a => a.AdDynamicValues)
                    .ThenInclude(v => v.Field)
                .Include(a => a.Favorites)
                .FirstOrDefaultAsync(a =>
                    a.Id == adId &&
                    (a.Status == "Active" ||
                     (currentUserId.HasValue && a.UserId == currentUserId)));

            if (ad == null)
                return null;

            // 🔥 TRACK VIEW (Skip if updating)
            if (currentUserId.HasValue && !skipTracking)
            {
                _context.UserAdActivities.Add(new UserAdActivity
                {
                    UserId = currentUserId.Value,
                    AdId = adId,
                    ActionType = "View"
                });
                await _context.SaveChangesAsync();
            }

            var dto = _mapper.Map<AdResponseWithDynamicDto>(ad);

            // Get full image URLs
            var imageUrls = ad.AdImages
                .OrderBy(i => i.DisplayOrder)
                .Select(i => GetFullImageUrl(i.ImageUrl))
                .ToList();

            dto.ImageUrls = imageUrls;
            dto.PrimaryImageUrl = imageUrls.FirstOrDefault();

            dto.DynamicFields = ad.AdDynamicValues
                .Where(v => v.Field != null)
                .Select(v => new DynamicFieldResponseDto
                {
                    Id = v.Field.Id,
                    CategoryId = v.Field.CategoryId,
                    Label = v.Field.Label,
                    FieldType = v.Field.FieldType,
                    Options = v.Field.Options,
                    IsRequired = v.Field.IsRequired ?? false,
                    Value = v.FieldValue
                }).ToList();

            dto.IsFavorited = currentUserId.HasValue &&
                              ad.Favorites.Any(f => f.UserId == currentUserId);

            return dto;
        }

        // =====================================================
        // 3️⃣ CREATE AD (POST AD)
        // =====================================================
        public async Task<AdResponseWithDynamicDto> CreateAdAsync(
            AdCreateWithDynamicDto dto,
            int userId)
        {
            var ad = new Ad
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Location = dto.Location,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CategoryId = dto.CategoryId,
                UserId = userId,
                Status = "Active", // Changed from "Pending" to "Active"
                CreatedAt = DateTime.Now
            };

            _context.Ads.Add(ad);
            await _context.SaveChangesAsync();

            // 📸 Images
            if (dto.Images != null && dto.Images.Any())
                await SaveImagesAsync(ad.Id, dto.Images);

            // 🧩 Dynamic Fields
            if (dto.DynamicValues != null)
            {
                foreach (var field in dto.DynamicValues)
                {
                    _context.AdDynamicValues.Add(new AdDynamicValue
                    {
                        AdId = ad.Id,
                        FieldId = field.FieldId,
                        FieldValue = field.Value
                    });
                }
            }

            await _context.SaveChangesAsync();
            return (await GetAdDetailsAsync(ad.Id, userId, true))!;
        }

        // =====================================================
        // 4️⃣ UPDATE AD (OWNER)
        // =====================================================
        public async Task<AdResponseWithDynamicDto> UpdateAdAsync(
            int adId,
            AdUpdateWithDynamicDto dto,
            int userId)
        {
            var ad = await _context.Ads
                .Include(a => a.AdImages)
                .Include(a => a.AdDynamicValues)
                .FirstOrDefaultAsync(a => a.Id == adId);

            if (ad == null)
                throw new KeyNotFoundException();

            if (ad.UserId != userId)
                throw new UnauthorizedAccessException();

            ad.Title = dto.Title ?? ad.Title;
            ad.Description = dto.Description ?? ad.Description;
            ad.Price = dto.Price ?? ad.Price;
            ad.Location = dto.Location ?? ad.Location;
            ad.Latitude = dto.Latitude ?? ad.Latitude;
            ad.Longitude = dto.Longitude ?? ad.Longitude;

            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (dto.Status.Equals("Sold", StringComparison.OrdinalIgnoreCase))
                    ad.Status = "Sold";
                else if (dto.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    ad.Status = "Active";
            }

            // Update dynamic fields
            if (dto.DynamicValues != null)
            {
                _context.AdDynamicValues.RemoveRange(ad.AdDynamicValues);
                foreach (var field in dto.DynamicValues)
                {
                    _context.AdDynamicValues.Add(new AdDynamicValue
                    {
                        AdId = adId,
                        FieldId = field.FieldId,
                        FieldValue = field.Value
                    });
                }
            }

            // Update images
            if (dto.NewImages != null && dto.NewImages.Any())
            {
                _context.AdImages.RemoveRange(ad.AdImages);
                await SaveImagesAsync(ad.Id, dto.NewImages);
            }

            await _context.SaveChangesAsync();
            return (await GetAdDetailsAsync(ad.Id, userId, true))!;
        }

        // =====================================================
        // 5️⃣ OWNER PREVIEW
        // =====================================================
        public async Task<AdResponseWithDynamicDto?> GetAdForOwnerAsync(
            int adId,
            int userId)
        {
            var ad = await _context.Ads
                .FirstOrDefaultAsync(a => a.Id == adId && a.UserId == userId);

            return ad == null ? null : await GetAdDetailsAsync(adId, userId, true);
        }

        // =====================================================
        // 5.5️⃣ GET USER'S OWN ADS (FOR MANAGEMENT)
        // =====================================================
        public async Task<PagedList<AdResponseDto>> GetUserAdsAsync(
            int userId,
            int page,
            int pageSize)
        {
            var adsPaged = await _adRepo.GetUserAdsAsync(userId, page, pageSize);

            var result = adsPaged.Items.Select(ad =>
            {
                var responseDto = _mapper.Map<AdResponseDto>(ad);

                // Get full image URLs
                var imageUrls = ad.AdImages
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => GetFullImageUrl(i.ImageUrl))
                    .ToList();

                responseDto.ImageUrls = imageUrls;
                responseDto.PrimaryImageUrl = imageUrls.FirstOrDefault();

                // Calculate time ago
                if (ad.CreatedAt.HasValue)
                {
                    var diff = DateTime.Now - ad.CreatedAt.Value;
                    if (diff.TotalDays < 1)
                        responseDto.PostedTimeAgo = "Today";
                    else if (diff.TotalDays < 2)
                        responseDto.PostedTimeAgo = "Yesterday";
                    else
                        responseDto.PostedTimeAgo = $"{(int)diff.TotalDays} days ago";
                }

                if (!string.IsNullOrEmpty(ad.Location))
                {
                    responseDto.ShortLocation = ad.Location.Split(',')[0].Trim();
                }

                return responseDto;
            }).ToList();

            return new PagedList<AdResponseDto>(
                result,
                adsPaged.TotalCount,
                adsPaged.Page,
                adsPaged.PageSize);
        }

        public async Task<bool> DeleteAdAsync(int adId, int userId)
        {
            return await _adRepo.DeleteAsync(adId, userId);
        }

        // =====================================================
        // 6️⃣ FAVORITE TOGGLE + TRACKING
        // =====================================================
        public async Task ToggleFavoriteAsync(int adId, int userId)
        {
            var added = await _favoriteRepo.ToggleAsync(adId, userId);

            if (added)
            {
                _context.UserAdActivities.Add(new UserAdActivity
                {
                    UserId = userId,
                    AdId = adId,
                    ActionType = "Favorite"
                });
                await _context.SaveChangesAsync();
            }
        }

        // =====================================================
        // 🔍 7️⃣ OLX SEARCH ENGINE (USED FOR HOME FEED TOO)
        // =====================================================
        public async Task<PagedList<AdResponseDto>> SearchAdsAsync(
            OlxSearchDto dto,
            int? currentUserId)
        {
            // Call the proximity-aware repository method
            var adsPaged = await _adRepo.SearchAdsAsync(dto);

            var result = adsPaged.Items.Select(ad =>
            {
                var responseDto = _mapper.Map<AdResponseDto>(ad);

                // Get full image URLs
                var imageUrls = ad.AdImages
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => GetFullImageUrl(i.ImageUrl))
                    .ToList();

                responseDto.ImageUrls = imageUrls;
                responseDto.PrimaryImageUrl = imageUrls.FirstOrDefault();

                // Calculate time ago
                if (ad.CreatedAt.HasValue)
                {
                    var diff = DateTime.Now - ad.CreatedAt.Value;
                    if (diff.TotalDays < 1)
                        responseDto.PostedTimeAgo = "Today";
                    else if (diff.TotalDays < 2)
                        responseDto.PostedTimeAgo = "Yesterday";
                    else
                        responseDto.PostedTimeAgo = $"{(int)diff.TotalDays} days ago";
                }

                // GEOLOCATION: Map coordinates and calculate distance back if searched
                responseDto.Latitude = ad.Latitude;
                responseDto.Longitude = ad.Longitude;

                if (dto.Latitude.HasValue && dto.Longitude.HasValue && ad.Latitude.HasValue && ad.Longitude.HasValue)
                {
                    responseDto.DistanceKm = Math.Round(GetDistance(
                        dto.Latitude.Value, dto.Longitude.Value,
                        ad.Latitude.Value, ad.Longitude.Value), 1);
                }

                // Short location (first part)
                if (!string.IsNullOrEmpty(ad.Location))
                {
                    responseDto.ShortLocation = ad.Location.Split(',')[0].Trim();
                }

                responseDto.IsFavorited = currentUserId.HasValue &&
                                          ad.Favorites.Any(f => f.UserId == currentUserId);

                // Get highlights from dynamic fields
                var highlights = ad.AdDynamicValues
                    .Where(v => !string.IsNullOrEmpty(v.FieldValue))
                    .Take(3) // Show only 3 highlights
                    .ToDictionary(v => v.Field.Label, v => v.FieldValue!);

                responseDto.Highlights = highlights;

                return responseDto;
            }).ToList();

            return new PagedList<AdResponseDto>(
                result,
                adsPaged.TotalCount,
                adsPaged.Page,
                adsPaged.PageSize);
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = (lat2 - lat1) * (Math.PI / 180.0);
            double dLon = (lon2 - lon1) * (Math.PI / 180.0);
            double rLat1 = (lat1) * (Math.PI / 180.0);
            double rLat2 = (lat2) * (Math.PI / 180.0);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(rLat1) * Math.Cos(rLat2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return 6371 * c;
        }

        // =====================================================
        // 8️⃣ SIDEBAR FILTER DATA
        // =====================================================
        public async Task<CategoryFilterOptionsDto?> GetCategoryFiltersAsync(
            int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.DynamicFields)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                return null;

            var ads = _context.Ads
                .Include(a => a.AdDynamicValues)
                    .ThenInclude(v => v.Field)
                .Where(a => a.Status == "Active" && a.CategoryId == categoryId);

            var prices = await ads.Select(a => a.Price).ToListAsync();

            var locations = await ads
                .Select(a => a.Location)
                .Distinct()
                .Take(20)
                .ToListAsync();

            var fieldOptions = new Dictionary<string, List<string>>();

            foreach (var field in category.DynamicFields)
            {
                var values = await ads
                    .SelectMany(a => a.AdDynamicValues)
                    .Where(v => v.FieldId == field.Id && v.FieldValue != null)
                    .Select(v => v.FieldValue!)
                    .Distinct()
                    .ToListAsync();

                fieldOptions[field.Label] = values;
            }

            return new CategoryFilterOptionsDto
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                ParentCategoryId = category.ParentId,
                MinPrice = prices.Any() ? prices.Min() : 0,
                MaxPrice = prices.Any() ? prices.Max() : 0,
                AvgPrice = prices.Any() ? prices.Average() : 0,
                PopularLocations = locations,
                FieldOptions = fieldOptions,
                TotalAds = await ads.CountAsync()
            };
        }

        // =====================================================
        // 📸 IMAGE HELPER
        // =====================================================
        private async Task SaveImagesAsync(int adId, List<IFormFile> images)
        {
            var path = Path.Combine(
                _env.WebRootPath,
                "uploads",
                "ads",
                adId.ToString());

            Directory.CreateDirectory(path);

            int order = 0;

            foreach (var image in images.Take(8))
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(path, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                _context.AdImages.Add(new AdImage
                {
                    AdId = adId,
                    ImageUrl = $"/uploads/ads/{adId}/{fileName}",
                    DisplayOrder = order,
                    IsPrimary = order == 0
                });

                order++;
            }
        }
    }
}