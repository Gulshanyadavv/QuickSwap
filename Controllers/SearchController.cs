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
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly IAdService _adService;

        public SearchController(IAdService adService)
        {
            _adService = adService;
        }

        // =========================================
        // 1. GLOBAL SEARCH (HOME SEARCH BAR)
        // =========================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedList<AdResponseDto>>> SearchAds(
            [FromQuery] OlxSearchDto dto)
        {
            int? userId = User.Identity?.IsAuthenticated == true
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                : null;

            var result = await _adService.SearchAdsAsync(dto, userId);
            return Ok(result);
        }

        // =========================================
        // 2. CATEGORY / SUBCATEGORY SEARCH
        // =========================================
        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedList<AdResponseDto>>> SearchByCategory(
            int categoryId,
            [FromQuery] OlxSearchDto dto)
        {
            dto.CategoryId = categoryId;

            int? userId = User.Identity?.IsAuthenticated == true
                ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
                : null;

            var result = await _adService.SearchAdsAsync(dto, userId);
            return Ok(result);
        }

        // =========================================
        // 3. SIDEBAR FILTERS (OLX LEFT PANEL)
        // =========================================
        [HttpGet("filters/{categoryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryFilterOptionsDto>> GetFilters(
            int categoryId)
        {
            var filters = await _adService.GetCategoryFiltersAsync(categoryId);

            if (filters == null)
                return NotFound();

            return Ok(filters);
        }
    }
}
