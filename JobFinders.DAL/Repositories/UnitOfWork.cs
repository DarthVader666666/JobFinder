using JobFinders.Domain.Entities;
using JobFinders.Domain.Interfaces;

namespace JobFinders.DAL.Repositories
{
    public class UnitOfWork(JobFinderDbContext dbContext, IRepository<User> users, IRepository<Role> roles) : IUnitOfWork
    {
        public IRepository<User> Users { get; set; } = users;
        public IRepository<Role> Roles { get; set; } = roles;
 
        public Task<int> SaveChangesAsync()
        {
            return dbContext.SaveChangesAsync();
        }
    }
}