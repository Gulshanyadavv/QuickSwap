using O_market.DTO;
using O_market.DTOs;
using O_market.Models;

namespace O_market.Services
{
    public interface IFavoriteService
    {
        Task<bool> ToggleFavoriteAsync(FavoriteToggleDto dto, int userId);

        Task<PagedList<FavoriteResponseDto>> GetFavoritesAsync(FavoriteListDto listDto, int userId);

        Task<bool> IsFavoritedAsync(int adId, int userId);
    }
}