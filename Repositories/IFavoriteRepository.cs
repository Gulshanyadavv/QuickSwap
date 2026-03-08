using O_market.DTO;

using O_market.Models;

namespace O_market.Repositories
{
    public interface IFavoriteRepository
    {
        Task<bool> ToggleAsync(int adId, int userId);
        Task<PagedList<Favorite>> GetFavoritesAsync(FavoriteListDto listDto, int userId);
        Task<bool> IsFavoritedAsync(int adId, int userId);
    }
}