namespace O_market.Services
{
    public interface IEmailService
    {
        Task<(bool Success, string Message)> SendOtpAsync(string email, string otp);
    }
}