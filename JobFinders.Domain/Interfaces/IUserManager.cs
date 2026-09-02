using JobFinders.Domain.Entities;

namespace JobFinders.Domain.Interfaces
{
    public interface IUserManager
    {
        bool TryGetUserByEmail(string email, out User? user);
        Task<bool> RegisterUser(string? email, string? password);
        Task ConfirmUser(User? user);
        Task<string> GenerateCodeAsync(User? user);
        bool IsCodeExpired(User? user, out ConfirmationCode? code);
        string? GetCode(User? user);
    }
}
