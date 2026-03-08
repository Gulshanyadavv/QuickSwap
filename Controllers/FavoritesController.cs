using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O_market.DTO;
using O_market.DTOs;
using O_market.Models;
using O_market.Services;

namespace O_market.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _service;
        private int CurrentUserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        public FavoritesController(IFavoriteService service) { _service = service; }
        [HttpPost]
        public async Task<IActionResult> Toggle([FromBody] FavoriteToggleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var added = await _service.ToggleFavoriteAsync(dto, CurrentUserId);
            var message = added ? "Added to favorites" : "Removed from favorites";
            return Ok(new { message, added });
        }


        [HttpDelete("{adId}")]
        public async Task<IActionResult> RemoveFavorite(int adId)
        {
            var dto = new FavoriteToggleDto { AdId = adId };
            var added = await _service.ToggleFavoriteAsync(dto, CurrentUserId);
            if (!added) // It was removed
            {
                return Ok(new { message = "Removed from favorites", added = false });
            }
            return Ok(new { message = "Was not in favorites", added = true });
        }
        [HttpGet]
        public async Task<ActionResult<PagedList<FavoriteResponseDto>>> Get([FromQuery] FavoriteListDto listDto)
        {
            var favorites = await _service.GetFavoritesAsync(listDto, CurrentUserId);
            return Ok(favorites);
        }

        [HttpGet("check/{adId}")]
        public async Task<ActionResult<bool>> IsFavorited(int adId)
        {
            var isFavorited = await _service.IsFavoritedAsync(adId, CurrentUserId);
            return Ok(isFavorited);
        }


    }
}