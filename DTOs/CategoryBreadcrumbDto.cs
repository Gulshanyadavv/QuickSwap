namespace O_market.DTO
{

    public class CategoryBreadcrumbDto
    {
        public int CategoryId { get; set; }
        public List<BreadcrumbDto> Breadcrumbs { get; set; } = new();
        public string CategoryName { get; set; } = null!;
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
    }

}
