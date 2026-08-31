using JobFinders.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace JobFinders.DAL
{
    public class JobFinderDbContext: DbContext
    {
        private const int maxLength = 300;

        public JobFinderDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(user =>
            {
                user.HasKey(u => u.UserId);
                user.Property(u => u.UserId).ValueGeneratedOnAdd();
                user.HasIndex(u => u.Email).IsUnique();
                user.Property(u => u.Email).IsRequired(true).HasMaxLength(maxLength);
                user.Property(u => u.Name).HasMaxLength(maxLength);
                user.Property(u => u.Password).HasMaxLength(maxLength);
            });

            modelBuilder.Entity<Role>(role =>
            {
                role.HasKey(u => u.RoleId);
                role.Property(r => r.RoleName).IsRequired(true).HasMaxLength(maxLength);
                role.HasData(
                    new Role { RoleId = 1, RoleName = "User" },
                    new Role { RoleId = 2, RoleName = "Admin" },
                    new Role { RoleId = 3, RoleName = "Owner" }
                    );
            });

            modelBuilder.Entity<UserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.User)
                        .WithMany(u => u.UserRoles)
                        .HasForeignKey(ur => ur.UserId);

                userRole.HasOne(ur => ur.Role)
                        .WithMany(r => r.UserRoles)
                        .HasForeignKey(ur => ur.RoleId);
            });

            modelBuilder.Entity<ConfirmationCode>(code =>
            {
                code.HasKey(c => c.CodeId);
                code.Property(c => c.CodeId).ValueGeneratedOnAdd();
                code.HasOne(c => c.User).WithOne(u => u.ConfirmationCode).HasForeignKey<ConfirmationCode>(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
                code.Property(c => c.DateGenerated).IsRequired(true);
                code.Property(c => c.Code).IsRequired(true);
            });
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<ConfirmationCode> ConfirmationCodes { get; set; }
    }
}
