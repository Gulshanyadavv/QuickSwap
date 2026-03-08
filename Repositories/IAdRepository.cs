using O_market.DTO;
using O_market.DTOs;
using O_market.Models;

namespace O_market.Repositories
{
    public interface IAdRepository
    {
        // Basic operations
        Task<Ad?> GetByIdAsync(int id, int? userId = null);
        Task<PagedList<Ad>> SearchAdsAsync(OlxSearchDto searchDto);
        Task<Ad> CreateAsync(Ad ad);
        Task<Ad> UpdateAsync(Ad ad);
        Task<bool> DeleteAsync(int id, int userId);

        // Image operations
        Task AddImagesAsync(int adId, List<AdImage> images);
        Task<List<AdImage>> GetImagesAsync(int adId);
        Task DeleteImagesAsync(int adId);

        // User operations
        Task<bool> IsOwnedByUserAsync(int adId, int userId);
        Task<PagedList<Ad>> GetUserAdsAsync(int userId, int page, int pageSize);

        // Favorite operations
        Task ToggleFavoriteAsync(int adId, int userId);

        // Dynamic field operations
        Task AddDynamicValuesAsync(int adId, List<AdDynamicValue> dynamicValues);
        Task<List<AdDynamicValue>> GetDynamicValuesAsync(int adId);
    }
}