namespace O_market.DTOs
{
    public class RegistrationResponseDto
    {
        public bool Success { get; set; }
        public int UserId { get; set; }
        public string? ErrorMessage { get; set; }
        public string Message { get; set; } = "Registration completed successfully";
    }
}
