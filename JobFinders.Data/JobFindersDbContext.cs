using JobFinders.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace JobFinders.Data
{
    public class JobFindersDbContext: DbContext
    {
        public JobFindersDbContext(DbContextOptions options) : base(options)
        {
            
        }

        public DbSet<JobFinder> JobFinders { get; set; }
    }
}
