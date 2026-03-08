namespace O_market.DTOs.Otp
{
    public class PendingRegistrationDto
    {
        public bool Success { get; set; }
        public int VerificationId { get; set; }
        public string? Email { get; set; }
        public DateTime? OtpExpiry { get; set; }
        public int? AttemptsLeft { get; set; }
        public int? CooldownSeconds { get; set; }
        public string? ErrorMessage { get; set; }
        public string Message { get; set; } = "OTP sent successfully. Please verify.";
    }
}
