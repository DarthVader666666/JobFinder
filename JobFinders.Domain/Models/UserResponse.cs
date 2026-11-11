namespace JobFinders.Domain.Models
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string[]? Roles { get; set; }
    }
}
