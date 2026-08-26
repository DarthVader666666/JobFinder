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
                user.HasIndex(u => u.Email).IsUnique();
                user.Property(u => u.Email).IsRequired(true).HasMaxLength(maxLength);
                user.Property(u => u.Name).IsRequired(true).HasMaxLength(maxLength);
                user.Property(u => u.Password).IsRequired(true).HasMaxLength(maxLength);
            });

            modelBuilder.Entity<Role>(role =>
            {
                role.HasKey(u => u.RoleId);
                role.Property(r => r.RoleName).IsRequired(true).HasMaxLength(maxLength);
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
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
    }
}
