using AutoMapper;
using O_market.DTO;
using O_market.DTOs;
using O_market.Models;
using O_market.Repositories;
using O_market.Services;

namespace O_market.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _repo;
        private readonly IMapper _mapper;

        public FavoriteService(IFavoriteRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<bool> ToggleFavoriteAsync(FavoriteToggleDto dto, int userId)
        {
            return await _repo.ToggleAsync(dto.AdId, userId);
        }

        public async Task<PagedList<FavoriteResponseDto>> GetFavoritesAsync(
    FavoriteListDto listDto,
    int userId
)
        {
            var pagedFavorites = await _repo.GetFavoritesAsync(listDto, userId);

            var baseUrl = "https://localhost:7126";

            var result = pagedFavorites.Items.Select(f =>
            {
                var images = f.Ad.AdImages
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => i.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) 
                                 ? i.ImageUrl 
                                 : $"{baseUrl}/{i.ImageUrl.TrimStart('/')}")
                    .ToList();

                return new FavoriteResponseDto
                {
                    Id = f.Id,
                    Ad = new AdResponseDto
                    {
                        Id = f.Ad.Id,
                        Title = f.Ad.Title,
                        Price = f.Ad.Price,
                        Location = f.Ad.Location,
                        ShortLocation = f.Ad.Location,
                        PostedTimeAgo = "Recently",
                        IsFavorited = true,
                        PrimaryImageUrl = images.FirstOrDefault(),
                        ImageUrls = images
                    },
                    PrimaryImageUrl = images.FirstOrDefault(),
                    ImageUrls = images
                };
            }).ToList();

            return new PagedList<FavoriteResponseDto>(
                result,
                pagedFavorites.TotalCount,
                pagedFavorites.Page,
                pagedFavorites.PageSize
            );
        }



        public async Task<bool> IsFavoritedAsync(int adId, int userId)
        {
            return await _repo.IsFavoritedAsync(adId, userId);
        }


    }
}
