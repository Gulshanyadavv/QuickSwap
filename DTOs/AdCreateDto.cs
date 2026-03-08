using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace O_market.DTOs
{
    public class AdCreateDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title must be max 200 characters.")]
        public string Title { get; set; } = null!;

        [StringLength(4000, ErrorMessage = "Description must be max 4000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(100, ErrorMessage = "Location must be max 100 characters.")]
        public string Location { get; set; } = null!;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required(ErrorMessage = "Category ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Valid category ID required.")]
        public int CategoryId { get; set; }

        // NEW: For multi-image upload (multipart/form-data)
        public List<IFormFile>? Images { get; set; }
        // Limit to 10 in service

    }
}