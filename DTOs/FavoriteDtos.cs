using System.ComponentModel.DataAnnotations;

namespace O_market.DTOs
{
    // Input: Toggle (just adId)
    public class FavoriteToggleDto
    {
        [Required(ErrorMessage = "Ad ID required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Valid ad ID required.")]
        public int AdId { get; set; }
    }
}
