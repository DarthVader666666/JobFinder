using JobFinders.Domain.Entities;

namespace JobFinders.Domain.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User? user);
    }
}
