using System.ComponentModel.DataAnnotations;

namespace O_market.DTOs
{
    // Input: Create new category
    public class CategoryCreateDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must be max 100 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description max 500 characters.")]
        public string? Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Valid parent category ID required.")]
        public int? ParentId { get; set; }  // Null for root
    }
}
