using JobFinders.Domain.Entities;
using JobFinders.Domain.Interfaces;

namespace JobFinders.DAL.Repositories
{
    public class UnitOfWork(JobFinderDbContext dbContext, IRepository<User> users, IRepository<Role> roles, IRepository<ConfirmationCode> codes, IRepository<UserRole> userRoles) : IUnitOfWork
    {
        public IRepository<User> Users { get; set; } = users;
        public IRepository<Role> Roles { get; set; } = roles;
        public IRepository<UserRole> UserRoles { get; set; } = userRoles;
        public IRepository<ConfirmationCode> ConfirmationCodes { get; set; } = codes;

        public Task<int> SaveChangesAsync()
        {
            return dbContext.SaveChangesAsync();
        }
    }
}