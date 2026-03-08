using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace O_market.DTOs
{
    public class AdUpdateWithDynamicDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(4000)]
        public string? Description { get; set; }

        public decimal? Price { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public int? CategoryId { get; set; }

        // OLX allows updating images
        public List<IFormFile>? NewImages { get; set; }

        // Dynamic fields update
        public List<DynamicFieldValueDto>? DynamicValues { get; set; }

        // User action
        public string? Status { get; set; } // Active / Sold
    }
}
