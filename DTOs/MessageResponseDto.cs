namespace O_market.DTO
{
    public class MessageResponseDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }

        // Sender/Receiver info (partial for privacy)
        public int SenderId { get; set; }
        public string SenderUsername { get; set; } = null!;
        public int ReceiverId { get; set; }
        public string ReceiverUsername { get; set; } = null!;

        public int? AdId { get; set; }
        public string? AdTitle { get; set; }  // If tied to ad

        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; }
    }
}
