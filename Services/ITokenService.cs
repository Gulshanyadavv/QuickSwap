using O_market.Models;

namespace O_market.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}