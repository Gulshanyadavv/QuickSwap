using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O_market.DTO;
using O_market.DTOs;
using O_market.Models;
using O_market.Services;
using System.Security.Claims;

namespace O_market.Controllers
{
    [ApiController]
    [Route("api/ads")]
    public class AdsController : ControllerBase
    {
        private readonly IAdService _adService;

        public AdsController(IAdService adService)
        {
            _adService = adService;
        }

        // ===============================
        // 1. HOME FEED WITH FILTERS
        // ===============================
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedList<AdResponseDto>>> BrowseAds(
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? location = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? sortBy = "newest",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            int? userId = User.Identity?.IsAuthenticated == true
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                : null;

            var searchDto = new OlxSearchDto
            {
                Search = search,
                CategoryId = categoryId,
                Location = location,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy,
                Page = page,
                PageSize = pageSize,
                IncludeSubcategories = true // Default true for home feed
            };

            var result = await _adService.SearchAdsAsync(searchDto, userId);
            return Ok(result);
        }

        // ===============================
        // 2. VIEW SINGLE AD (DETAIL PAGE)
        // ===============================
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<AdResponseWithDynamicDto>> ViewAd(int id)
        {
            int? userId = User.Identity?.IsAuthenticated == true
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                : null;

            var ad = await _adService.GetAdDetailsAsync(id, userId);

            if (ad == null)
                return NotFound(new { Message = "Ad not found or inactive" });

            return Ok(ad);
        }

        // ===============================
        // 3. ADVANCED SEARCH (More filters)
        // ===============================
        [HttpGet("advanced")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedList<AdResponseDto>>> AdvancedSearch(
            [FromQuery] string? search = null,
            [FromQuery] List<int>? categoryIds = null,
            [FromQuery] bool includeSubcategories = true,
            [FromQuery] string? location = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? verifiedSellerOnly = null,
            [FromQuery] string? sortBy = "newest",
            [FromQuery] Dictionary<string, string>? dynamicFilters = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            int? userId = User.Identity?.IsAuthenticated == true
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                : null;

            var searchDto = new OlxSearchDto
            {
                Search = search,
                CategoryId = categoryIds?.FirstOrDefault(), // For compatibility
                IncludeSubcategories = includeSubcategories,
                Location = location,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy,
                DynamicFilters = dynamicFilters,
                Page = page,
                PageSize = pageSize
            };

            // For multiple category IDs
            if (categoryIds != null && categoryIds.Count > 0)
            {
                searchDto.CategoryId = categoryIds[0]; // First for main filter
            }

            var result = await _adService.SearchAdsAsync(searchDto, userId);
            return Ok(result);
        }

        // ===============================
        // 4. QUICK FAVORITE (FROM AD CARD)
        // ===============================
        [HttpPost("{id}/favorite")]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _adService.ToggleFavoriteAsync(id, userId);

            return Ok(new { Message = "Favorite updated" });
        }

        // ===============================
        // 5. MY ADS (FOR MANAGEMENT)
        // ===============================
        [HttpGet("my-ads")]
        [Authorize]
        public async Task<ActionResult<PagedList<AdResponseDto>>> GetMyAds(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _adService.GetUserAdsAsync(userId, page, pageSize);
            return Ok(result);
        }

        // ===============================
        // 6. DELETE AD
        // ===============================
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAd(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var success = await _adService.DeleteAdAsync(id, userId);

            if (!success)
                return NotFound(new { Message = "Ad not found or you don't have permission." });

            return NoContent();
        }
    }
}