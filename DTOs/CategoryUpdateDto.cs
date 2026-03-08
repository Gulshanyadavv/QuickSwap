using System.ComponentModel.DataAnnotations;

namespace O_market.DTO
{
    public class CategoryUpdateDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int? ParentId { get; set; }
    }
}
