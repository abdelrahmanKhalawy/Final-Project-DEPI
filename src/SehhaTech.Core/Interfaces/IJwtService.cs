using SehhaTech.Core.Models;

namespace SehhaTech.Core.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}