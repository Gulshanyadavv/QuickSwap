using System.ComponentModel.DataAnnotations;

namespace O_market.DTOs
{
    // Input: Send new message
    public class MessageCreateDto
    {
        [Required(ErrorMessage = "Content is required.")]
        [StringLength(2000, ErrorMessage = "Message max 2000 characters.")]
        public string Content { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Valid receiver ID required.")]
        public int ReceiverId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Valid ad ID required (optional).")]
        public int? AdId { get; set; }  // Ties to ad inquiry

        public IFormFile? Attachment { get; set; }
    }
}