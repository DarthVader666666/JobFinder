namespace JobFinders.Domain.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(string? userName, string? email, IEnumerable<string> roles);
    }
}
