using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace O_market.DTOs
{
    // Input DTO for creating ad with dynamic fields
    public class AdCreateWithDynamicDto
    {
        // Basic fields
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

        public List<IFormFile>? Images { get; set; }

        // Dynamic field values
        public List<DynamicFieldValueDto>? DynamicValues { get; set; }
    }

    // DTO for dynamic field value
    public class DynamicFieldValueDto
    {
        [Required(ErrorMessage = "Field ID is required.")]
        public int FieldId { get; set; }

        [Required(ErrorMessage = "Value is required.")]
        public string Value { get; set; } = null!;
    }

    // Output DTO for ad with dynamic fields
    public class AdResponseWithDynamicDto : AdResponseDto
    {
        public List<DynamicFieldResponseDto> DynamicFields { get; set; } = new();
    }

    public class DynamicFieldResponseDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Label { get; set; } = null!;
        public string FieldType { get; set; } = null!;
        public string? Options { get; set; }
        public bool IsRequired { get; set; }
        public string? Value { get; set; }
    }

    // DTO for creating/updating dynamic fields (Admin)
    public class DynamicFieldCreateDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Label { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string FieldType { get; set; } = null!; // text, number, dropdown, checkbox, radio

        public string? Options { get; set; } // JSON array for dropdown/radio

        public bool IsRequired { get; set; } = false;
    }

    public class DynamicFieldUpdateDto
    {
        [StringLength(100)]
        public string? Label { get; set; }

        [StringLength(50)]
        public string? FieldType { get; set; }

        public string? Options { get; set; }
        public bool? IsRequired { get; set; }
    }
}