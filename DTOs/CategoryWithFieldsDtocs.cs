namespace O_market.DTO
{
    // For category with dynamic fields
    public class CategoryWithFieldsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public List<CategoryWithFieldsDto> SubCategories { get; set; } = new();
        public List<DynamicFieldDto> DynamicFields { get; set; } = new();
    }
}