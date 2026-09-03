namespace JobFinders.Domain.Entities
{
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; } = 1;
        public User? User { get; set; }
        public Role? Role { get; set; }
    }
}
