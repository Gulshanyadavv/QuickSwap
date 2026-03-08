namespace O_market.DTOs
{
    public class InboxChatDto
    {
        public int AdId { get; set; }
        public string AdTitle { get; set; } = null!;
        public string SellerName { get; set; } = null!;

        public int OtherUserId { get; set; }
        public string OtherUsername { get; set; } = null!;

        public string LastMessage { get; set; } = null!;
        public DateTime LastMessageAt { get; set; }
    }
}
