using JobFinders.Domain.Models;

namespace JobFinders.Domain.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(UserResponse? user);
        Task RevokeRefreshToken(int userId);
    }
}
