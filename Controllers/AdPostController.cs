using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O_market.DTOs;
using O_market.Services;
using System.Security.Claims;

namespace O_market.Controllers
{
    [ApiController]
    [Route("api/ad-post")]
    [Authorize] // Login required
    public class AdPostController : ControllerBase
    {
        private readonly IAdService _adService;

        public AdPostController(IAdService adService)
        {
            _adService = adService;
        }

        // =========================================
        // 1. CREATE AD (WITH IMAGES + DYNAMIC FIELDS)
        // =========================================
        [HttpPost]
        public async Task<ActionResult<AdResponseWithDynamicDto>> CreateAd(
            [FromForm] AdCreateWithDynamicDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ad = await _adService.CreateAdAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetMyAd),
                new { id = ad.Id },
                ad
            );
        }

        // =========================================
        // 2. UPDATE AD (OWNER ONLY)
        // =========================================
        [HttpPut("{id}")]
        public async Task<ActionResult<AdResponseWithDynamicDto>> UpdateAd(
            int id,
            [FromForm] AdUpdateWithDynamicDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var updatedAd = await _adService.UpdateAdAsync(id, dto, userId);
                return Ok(updatedAd);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = "Ad not found" });
            }
        }

        // =========================================
        // 3. VIEW MY OWN AD (PREVIEW / EDIT)
        // =========================================
        [HttpGet("my/{id}")]
        public async Task<ActionResult<AdResponseWithDynamicDto>> GetMyAd(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ad = await _adService.GetAdForOwnerAsync(id, userId);

            if (ad == null)
                return NotFound();

            return Ok(ad);
        }
    }
}
