using JobFinders.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace JobFinders.Data
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
                user.HasIndex(u => u.Email).IsUnique();
                user.Property(u => u.Email).IsRequired(true).HasMaxLength(maxLength);
                user.Property(u => u.Name).IsRequired(true).HasMaxLength(maxLength);
                user.Property(u => u.Password).IsRequired(true).HasMaxLength(maxLength);
                user.HasKey(u => u.UserId);
            });
        }

        public virtual DbSet<User> Users { get; set; }
    }
}
