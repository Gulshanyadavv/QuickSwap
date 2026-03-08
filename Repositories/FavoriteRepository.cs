using Microsoft.EntityFrameworkCore;
using O_market.DTO;
using O_market.DTOs;
using O_market.Models;
using O_market.Repositories;

namespace O_market.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly OlxdbContext _context;

        public FavoriteRepository(OlxdbContext context) { _context = context; }

        public async Task<bool> ToggleAsync(int adId, int userId)
        {
            var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.AdId == adId && f.UserId == userId);
            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
            }
            else
            {
                _context.Favorites.Add(new Favorite { AdId = adId, UserId = userId });
            }
            await _context.SaveChangesAsync();
            return favorite == null;  // True if added, false if removed
        }

        public async Task<PagedList<Favorite>> GetFavoritesAsync(FavoriteListDto listDto, int userId)
        {
            var query = _context.Favorites
                .Include(f => f.Ad).ThenInclude(a => a.AdImages)  // Load ad details
                .Include(f => f.Ad.User)
                .Include(f => f.Ad.Category)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.Id);  // Recent first

            var total = await query.CountAsync();
            var favorites = await query
                .Skip((listDto.Page - 1) * listDto.PageSize)
                .Take(listDto.PageSize)
                .ToListAsync();

            return new PagedList<Favorite>(favorites, total, listDto.Page, listDto.PageSize);
        }

        public async Task<bool> IsFavoritedAsync(int adId, int userId)
        {
            return await _context.Favorites.AnyAsync(f => f.AdId == adId && f.UserId == userId);
        }
    }
}