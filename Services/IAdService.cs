using O_market.DTO;
using O_market.DTOs;
using O_market.Models;

namespace O_market.Services
{
    public interface IAdService
    {

        // =====================================================
        // HOME / BROWSE
        // =====================================================

        /// <summary>
        /// OLX Home feed (latest active ads)
        /// </summary>
        Task<PagedList<AdResponseDto>> GetLatestAdsAsync(
            int page,
            int pageSize,
            int? currentUserId);


        // =====================================================
        // VIEW AD
        // =====================================================

        /// <summary>
        /// View ad details (with dynamic fields + AI tracking)
        /// </summary>
        Task<AdResponseWithDynamicDto?> GetAdDetailsAsync(
            int adId,
            int? currentUserId,
            bool skipTracking = false);


        // =====================================================
        // POST / UPDATE AD
        // =====================================================

        /// <summary>
        /// Create new ad (Status = Pending)
        /// </summary>
        Task<AdResponseWithDynamicDto> CreateAdAsync(
            AdCreateWithDynamicDto dto,
            int userId);

        /// <summary>
        /// Update ad (owner only)
        /// </summary>
        Task<AdResponseWithDynamicDto> UpdateAdAsync(
            int adId,
            AdUpdateWithDynamicDto dto,
            int userId);

        /// <summary>
        /// Owner preview (even if Pending / Sold)
        /// </summary>
        Task<AdResponseWithDynamicDto?> GetAdForOwnerAsync(
            int adId,
            int userId);

        Task<PagedList<AdResponseDto>> GetUserAdsAsync(
            int userId,
            int page,
            int pageSize);

        Task<bool> DeleteAdAsync(int adId, int userId);


        // =====================================================
        // FAVORITES
        // =====================================================

        /// <summary>
        /// Toggle favorite (with AI tracking)
        /// </summary>
        Task ToggleFavoriteAsync(
            int adId,
            int userId);


        // =====================================================
        // SEARCH (OLX ENGINE)
        // =====================================================

        /// <summary>
        /// OLX global search + filters + sorting
        /// </summary>
        Task<PagedList<AdResponseDto>> SearchAdsAsync(
            OlxSearchDto dto,
            int? currentUserId);


        // =====================================================
        // CATEGORY FILTERS (SIDEBAR)
        // =====================================================

        /// <summary>
        /// Sidebar filters (price, location, dynamic fields)
        /// </summary>
        Task<CategoryFilterOptionsDto?> GetCategoryFiltersAsync(
         int categoryId);
    }
}
