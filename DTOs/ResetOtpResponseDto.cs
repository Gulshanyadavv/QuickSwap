namespace O_market.DTOs.Password
{
    public class ResetOtpResponseDto
    {
        public bool Success { get; set; }
        public string? TempToken { get; set; }
        public string? ErrorMessage { get; set; }
        public string Message { get; set; } = "OTP sent for password reset.";
    }
}
