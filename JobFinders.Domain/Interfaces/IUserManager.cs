using JobFinders.Domain.Entities;

namespace JobFinders.Domain.Interfaces
{
    public interface IUserManager
    {
        User? GetUserByEmail(string email);
        User? GetUserById(int id);
        User? GetUserByCode(string code);
        Task<bool> LogIn(User? user);
        Task<bool> Register(User user);
        bool DoesUserExist(string? email);
        Task<string> GenerateCodeAsync(string email);
        bool CodeExpired(string code);
    }
}
