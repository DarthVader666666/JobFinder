namespace JobFinders.Domain.Models
{
    public class UserResponse
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string[]? Roles { get; set; }
    }
}
