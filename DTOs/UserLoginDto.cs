using System.ComponentModel.DataAnnotations;

namespace O_market.DTOs.Auth
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Username or Email is required.")]
        public string Identifier { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6)]
        public string Password { get; set; } = null!;
    }
}
