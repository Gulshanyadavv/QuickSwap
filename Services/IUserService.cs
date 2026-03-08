// Services/IUserService.cs
using O_market.DTOs;
using O_market.DTOs.Auth;
using O_market.DTOs.Otp;
using O_market.DTOs.Password;
using O_market.Models;

namespace O_market.Services
{
    public interface IUserService
    {
        Task<PendingRegistrationDto> InitiateRegistrationAsync(UserRegistrationDto dto);
        Task<RegistrationResponseDto> VerifyOtpAndCompleteAsync(OtpVerificationDto dto);
        Task<ResendOtpResponseDto> ResendOtpAsync(ResendOtpDto dto);
        Task<AuthResponseDto?> AuthenticateAsync(UserLoginDto loginDto);
        Task<ResetOtpResponseDto> InitiatePasswordResetAsync(string identifier);
        Task<ResetPasswordVerifyResponseDto> VerifyResetOtpAsync(string identifier, string otp);
        Task<bool> VerifyOtpAndResetPasswordAsync(string identifier, string otp, string newPassword);
        Task<User?> GetUserByIdAsync(int userId);

        Task<bool> UpdateUserAsync(User user);
        Task<User?> GetUserByIdentifierAsync(string identifier);
        Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
