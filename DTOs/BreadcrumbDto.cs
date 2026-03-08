namespace O_market.DTO
{
    public class BreadcrumbDto
    {
        public string Text { get; set; } = null!;
        public string? Url { get; set; }
        public bool IsActive { get; set; }
    }
}
