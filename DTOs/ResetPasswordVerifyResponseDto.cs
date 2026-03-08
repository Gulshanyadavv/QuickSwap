
namespace O_market.DTOs.Password
{
    public class ResetPasswordVerifyResponseDto
    {
        public bool Success { get; set; }
        public string? TempToken { get; set; }
        public string? ErrorMessage { get; set; }
        public string Message { get; set; } = "OTP verified. You can reset password.";
    }
}
