using Microsoft.EntityFrameworkCore;
using O_market.Models;

namespace O_market.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly OlxdbContext _context;
        public UserRepository(OlxdbContext context) { _context = context; }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByIdentifierAsync(string identifier)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == identifier || u.Email == identifier);
        }

        public async Task<bool> ExistsByEmailOrUsernameAsync(string email, string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email || u.Username == username);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetByIdForVerificationAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> VerifyOtpAsync(int userId, string otp)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == userId &&
                u.Otp == otp &&
                u.OtpExpiry > DateTime.UtcNow &&
                !u.IsVerified);

            if (user != null)
            {
                user.IsVerified = true;
                user.Otp = null;
                user.OtpExpiry = null;
                user.OtpAttempts = 0;
                user.OtpSentAt = null;
                await _context.SaveChangesAsync();
                return true;
            }

            var failedUser = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == userId && !u.IsVerified);

            if (failedUser != null)
            {
                failedUser.OtpAttempts = (failedUser.OtpAttempts ?? 0) + 1;
                await _context.SaveChangesAsync();
            }

            return false;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateProfileAsync(int userId, string fullName, string email)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.FullName = fullName;
            user.Email = email;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}