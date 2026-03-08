namespace O_market.DTO
{
    public class MessageThreadDto
    {
        public int? AdId { get; set; }  // Filter by ad
        public int? OtherUserId { get; set; }  // Filter by user
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
