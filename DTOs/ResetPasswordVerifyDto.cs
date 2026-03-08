using System.ComponentModel.DataAnnotations;

namespace O_market.DTOs.Password
{
    public class ResetPasswordVerifyDto
    {
        [Required]
        public string Identifier { get; set; } = null!;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = null!;
    }
}

