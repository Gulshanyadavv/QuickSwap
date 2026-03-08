namespace O_market.DTO
{
    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public List<CategoryResponseDto> SubCategories { get; set; } = new();  // Children

        public List<DynamicFieldDto> DynamicFields { get; set; } = new List<DynamicFieldDto>();
    }
}
