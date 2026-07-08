using Microsoft.EntityFrameworkCore;

namespace JobFinders.Data
{
    public class JobFindersDbContext: DbContext
    {
        public JobFindersDbContext(DbContextOptions options) : base(options)
        {
            
        }
    }
}
