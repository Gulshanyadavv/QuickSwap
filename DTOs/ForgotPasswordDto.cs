using System.ComponentModel.DataAnnotations;

namespace O_market.DTOs.Password
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email or Username is required.")]
        public string Identifier { get; set; } = null!;
    }
}
