using System.ComponentModel.DataAnnotations;

namespace O_market.DTOs.Otp
{
    public class ResendOtpDto
    {
        [Required]
        public int VerificationId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
