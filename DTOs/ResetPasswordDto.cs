using System.ComponentModel.DataAnnotations;

namespace O_market.DTOs.Password
{
    public class ResetPasswordDto
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = null!;
    }
}
