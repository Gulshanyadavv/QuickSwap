using System.ComponentModel.DataAnnotations;

namespace O_market.DTOs.Otp
{
    public class OtpVerificationDto
    {
        [Required]
        public int VerificationId { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = null!;
    }
}
