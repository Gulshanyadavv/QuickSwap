// Repositories/IUserRepository.cs
using O_market.Models;

namespace O_market.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByIdentifierAsync(string identifier);
        Task<bool> ExistsByEmailOrUsernameAsync(string email, string username);
        Task<User> CreateAsync(User user);
        Task<User?> GetByIdForVerificationAsync(int id);
        Task<bool> VerifyOtpAsync(int userId, string otp);
        Task UpdateAsync(User user);

    }
}