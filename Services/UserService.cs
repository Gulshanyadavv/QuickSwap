using AutoMapper;
using Microsoft.Extensions.Logging;

using O_market.DTOs;
using O_market.DTOs.Auth;
using O_market.DTOs.Otp;
using O_market.DTOs.Password;
using O_market.Models;
using O_market.Repositories;
using O_market.Utilities;

namespace O_market.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repo, IMapper mapper,
                          ITokenService tokenService, IEmailService emailService,
                          ILogger<UserService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _tokenService = tokenService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ResetPasswordVerifyResponseDto> VerifyResetOtpAsync(string identifier, string otp)
        {
            var user = await _repo.GetByIdentifierAsync(identifier);

            if (user == null || user.Otp != otp || user.OtpExpiry < DateTime.UtcNow)
            {
                return new ResetPasswordVerifyResponseDto
                {
                    Success = false,
                    ErrorMessage = "Invalid or expired OTP."
                };
            }

            var tempToken = _tokenService.CreateToken(user);

            return new ResetPasswordVerifyResponseDto
            {
                Success = true,
                TempToken = tempToken,
                Message = "OTP verified. Use the token to reset password."
            };
        }

        public async Task<ResendOtpResponseDto> ResendOtpAsync(ResendOtpDto dto)
        {
            try
            {
                var user = await _repo.GetByIdForVerificationAsync(dto.VerificationId);

                if (user == null)
                {
                    return new ResendOtpResponseDto
                    {
                        Success = false,
                        ErrorMessage = "User not found or already verified."
                    };
                }

                if (user.IsVerified)
                {
                    return new ResendOtpResponseDto
                    {
                        Success = false,
                        ErrorMessage = "Account is already verified."
                    };
                }

                if (user.OtpAttempts >= 5)
                {
                    return new ResendOtpResponseDto
                    {
                        Success = false,
                        ErrorMessage = "Maximum OTP attempts reached. Please contact support."
                    };
                }

                if (user.OtpSentAt.HasValue &&
                    (DateTime.UtcNow - user.OtpSentAt.Value).TotalMinutes < 2)
                {
                    var timeLeft = 120 - (int)(DateTime.UtcNow - user.OtpSentAt.Value).TotalSeconds;
                    return new ResendOtpResponseDto
                    {
                        Success = false,
                        ErrorMessage = $"Please wait {timeLeft} seconds before requesting new OTP."
                    };
                }

                var random = new Random();
                var otp = random.Next(100000, 999999).ToString();

                user.Otp = otp;
                user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
                user.OtpSentAt = DateTime.UtcNow;
                user.OtpAttempts = (user.OtpAttempts ?? 0) + 1;

                var (emailSuccess, emailMessage) = await _emailService.SendOtpAsync(dto.Email, otp);

                if (!emailSuccess)
                {
                    _logger.LogWarning("Email sending failed for OTP resend to {Email}", dto.Email);
                }

                await _repo.UpdateAsync(user);

                return new ResendOtpResponseDto
                {
                    Success = true,
                    VerificationId = user.Id,
                    OtpExpiry = user.OtpExpiry,
                    AttemptsLeft = 5 - (user.OtpAttempts ?? 0),
                    Message = emailSuccess
                        ? "New OTP sent to your email. Please check your inbox."
                        : "OTP resend initiated. Check console/logs for OTP.",
                    CooldownSeconds = 120
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resend OTP failed for VerificationId: {Id}", dto.VerificationId);
                return new ResendOtpResponseDto
                {
                    Success = false,
                    ErrorMessage = "Failed to resend OTP. Please try again."
                };
            }
        }

        public async Task<PendingRegistrationDto> InitiateRegistrationAsync(UserRegistrationDto dto)
        {
            if (await _repo.ExistsByEmailOrUsernameAsync(dto.Email, dto.Username))
            {
                _logger.LogWarning("Registration failed: Username '{Username}' or Email '{Email}' already exists.", dto.Username, dto.Email);
                return new PendingRegistrationDto
                {
                    Success = false,
                    ErrorMessage = "Username or email already exists."
                };
            }

            var user = _mapper.Map<User>(dto);
            user.PasswordHash = PasswordHasher.HashPassword(dto.Password);
            user.Role = "Seller";
            user.CreatedAt = DateTime.UtcNow;
            user.IsVerified = false;

            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();

            user.Otp = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            user.OtpSentAt = DateTime.UtcNow;
            user.OtpAttempts = 1;

            try
            {
                var (emailSuccess, emailMessage) = await _emailService.SendOtpAsync(dto.Email, otp);

                if (!emailSuccess)
                {
                    _logger.LogWarning("Email sending failed, but saving user anyway for testing");
                }

                var createdUser = await _repo.CreateAsync(user);

                return new PendingRegistrationDto
                {
                    Success = true,
                    VerificationId = createdUser.Id,
                    Email = createdUser.Email,
                    OtpExpiry = createdUser.OtpExpiry,
                    AttemptsLeft = 4,
                    Message = emailSuccess
                        ? "OTP sent to your email. Please check your inbox."
                        : "Registration successful. Check console/logs for OTP.",
                    CooldownSeconds = 120
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Username}", dto.Username);
                return new PendingRegistrationDto
                {
                    Success = false,
                    ErrorMessage = "Registration failed. Please try again."
                };
            }
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _repo.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, "User not found");
                }

                // Verify current password
                if (!PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
                {
                    return (false, "Current password is incorrect");
                }

                // Check if new password is same as old
                if (PasswordHasher.VerifyPassword(newPassword, user.PasswordHash))
                {
                    return (false, "New password must be different from current password");
                }

                // Update password
                user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                user.Otp = null; // Clear any existing OTP
                user.OtpExpiry = null;
                user.OtpAttempts = 0;
                user.OtpSentAt = null;

                await _repo.UpdateAsync(user);

                _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                return (true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return (false, "An error occurred while changing password");
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                await _repo.UpdateAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", user.Id);
                return false;
            }
        }
        public async Task<User?> GetUserByIdentifierAsync(string identifier)
        {
            return await _repo.GetByIdentifierAsync(identifier);
        }

        public async Task<RegistrationResponseDto> VerifyOtpAndCompleteAsync(OtpVerificationDto dto)
        {
            var isValid = await _repo.VerifyOtpAsync(dto.VerificationId, dto.Otp);
            if (!isValid)
            {
                var user = await _repo.GetByIdForVerificationAsync(dto.VerificationId);
                if (user != null)
                {
                    var attemptsLeft = 5 - (user.OtpAttempts ?? 0);
                    return new RegistrationResponseDto
                    {
                        Success = false,
                        ErrorMessage = attemptsLeft > 0
                            ? $"Invalid or expired OTP. {attemptsLeft} attempts left."
                            : "Maximum OTP attempts reached. Please resend OTP."
                    };
                }

                return new RegistrationResponseDto
                {
                    Success = false,
                    ErrorMessage = "Invalid or expired OTP."
                };
            }

            var verifiedUser = await _repo.GetByIdAsync(dto.VerificationId);

            verifiedUser!.OtpAttempts = 0;
            verifiedUser.OtpSentAt = null;
            await _repo.UpdateAsync(verifiedUser);

            return new RegistrationResponseDto
            {
                Success = true,
                UserId = verifiedUser.Id,
                Message = "Email verified successfully! You can now login."
            };
        }

        public async Task<AuthResponseDto?> AuthenticateAsync(UserLoginDto loginDto)
        {
            var user = await _repo.GetByIdentifierAsync(loginDto.Identifier);
            if (user == null || !user.IsVerified || !PasswordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            var token = _tokenService.CreateToken(user);
            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Message = "Login successful"
            };
        }

        public async Task<ResetOtpResponseDto> InitiatePasswordResetAsync(string identifier)
        {
            var user = await _repo.GetByIdentifierAsync(identifier);
            if (user == null)
            {
                return new ResetOtpResponseDto
                {
                    Success = false,
                    ErrorMessage = "User not found."
                };
            }

            var otp = new Random().Next(100000, 999999).ToString();
            user.Otp = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            user.OtpSentAt = DateTime.UtcNow;
            user.OtpAttempts = (user.OtpAttempts ?? 0) + 1;

            Console.WriteLine($"🔐 Password Reset OTP for {user.Email}: {otp}");
            _logger.LogInformation("Password Reset OTP for {Email}: {Otp}", user.Email, otp);

            var (emailSuccess, emailMessage) = await _emailService.SendOtpAsync(user.Email, otp);

            await _repo.UpdateAsync(user);

            return new ResetOtpResponseDto
            {
                Success = true,
                TempToken = _tokenService.CreateToken(user),
                Message = emailSuccess
                    ? "OTP sent to your email. Please check your inbox."
                    : "OTP generated. Check console/logs for OTP.",
                ErrorMessage = null
            };
        }

        public async Task<bool> VerifyOtpAndResetPasswordAsync(string identifier, string otp, string newPassword)
        {
            var user = await _repo.GetByIdentifierAsync(identifier);
            if (user == null || user.Otp != otp || user.OtpExpiry < DateTime.UtcNow)
            {
                return false;
            }

            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            user.Otp = null;
            user.OtpExpiry = null;
            user.OtpAttempts = 0;
            user.OtpSentAt = null;

            await _repo.UpdateAsync(user);
            return true;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _repo.GetByIdAsync(userId);
        }
    }
}