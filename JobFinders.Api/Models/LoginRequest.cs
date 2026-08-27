namespace JobFinders.Api.Models
{
    public class LoginRequest
    {
        public string? Password { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
