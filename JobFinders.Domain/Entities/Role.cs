namespace JobFinders.Domain.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public string? RoleName { get; set; } = "User";
        public ICollection<UserRole>? UserRoles { get; set; }
    }
}
