// File: O_market/Controllers/UsersController_BAK.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O_market.DTOs;
using O_market.DTOs.Auth;
using O_market.DTOs.Otp;
using O_market.DTOs.Password;
using O_market.DTOs.Profile;
using O_market.Services;
using System.Security.Claims;

namespace O_market.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // Add this GET endpoint for profile
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { Message = "Invalid token" });

            int userId = int.Parse(userIdClaim.Value);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            return Ok(new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            });
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { Message = "Invalid token" });

            int userId = int.Parse(userIdClaim.Value);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            // Check if email is being changed and if it already exists
            if (user.Email != dto.Email)
            {
                var existingUser = await _userService.GetUserByIdentifierAsync(dto.Email);
                if (existingUser != null && existingUser.Id != userId)
                {
                    return BadRequest(new { Message = "Email is already in use by another account" });
                }
            }

            // Update user details
            user.FullName = dto.FullName;
            user.Email = dto.Email;

            // Save changes
            var success = await _userService.UpdateUserAsync(user);

            if (!success)
                return StatusCode(500, new { Message = "Failed to update profile" });

            return Ok(new
            {
                Message = "Profile updated successfully",
                User = new UserProfileDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.InitiateRegistrationAsync(dto);

            if (!result.Success)
                return BadRequest(new { Message = result.ErrorMessage });

            return StatusCode(201, result);
        }

        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.VerifyOtpAndCompleteAsync(dto);

            if (!result.Success)
                return BadRequest(new { Message = result.ErrorMessage });

            return Ok(result);
        }

        [HttpPost("resend-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.ResendOtpAsync(dto);

            if (!result.Success)
                return BadRequest(new { Message = result.ErrorMessage });

            return Ok(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authResult = await _userService.AuthenticateAsync(dto);

            if (authResult == null)
                return Unauthorized(new { Message = "Invalid credentials or unverified account" });

            return Ok(authResult);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.InitiatePasswordResetAsync(dto.Identifier);

            if (!result.Success)
                return BadRequest(new { Message = result.ErrorMessage });

            return Ok(new
            {
                TempToken = result.TempToken,
                Message = result.Message
            });
        }

        [HttpPost("verify-reset-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyResetOtp([FromBody] ResetPasswordVerifyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.VerifyResetOtpAsync(dto.Identifier, dto.Otp);

            if (!result.Success)
                return BadRequest(new { Message = result.ErrorMessage });

            return Ok(new
            {
                TempToken = result.TempToken,
                Message = result.Message
            });
        }

        [HttpPost("reset-password")]
        [Authorize]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { Message = "Validation failed", Errors = errors });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { Message = "Invalid or expired reset token" });

            int userId = int.Parse(userIdClaim.Value);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return BadRequest(new { Message = "User not found in system" });

            var success = await _userService.VerifyOtpAndResetPasswordAsync(
                user.Username,
                dto.Otp,
                dto.NewPassword
            );

            if (!success)
            {
                return BadRequest(new { Message = "Password reset failed. Possibly invalid OTP or expired code." });
            }

            return Ok(new { Message = "Password reset successful" });
        }

        // Add to O_market/Controllers/UsersController.cs
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { Message = "Invalid token" });

            int userId = int.Parse(userIdClaim.Value);

            var result = await _userService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);

            if (!result.Success)
                return BadRequest(new { Message = result.Message });

            return Ok(new { Message = result.Message });
        }
    }
}