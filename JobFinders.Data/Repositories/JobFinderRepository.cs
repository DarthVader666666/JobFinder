
using JobFinders.Data.Entities;

namespace JobFinders.Data.Repositories
{
    public class JobFinderRepository : IRepository<JobFinder>
    {
        private readonly JobFindersDbContext _dbContext;

        public JobFinderRepository(JobFindersDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<JobFinder>> GetAllAsync()
        {
            return Task.FromResult(_dbContext.JobFinders.ToList());
        }
    }
}
