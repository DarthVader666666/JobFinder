using JobFinders.Domain.Entities;
using JobFinders.Domain.Interfaces;

namespace JobFinders.DAL.Repositories
{
    public class UnitOfWork(JobFinderDbContext dbContext, IRepository<User> users, IRepository<Role> roles, IRepository<ConfirmationCode> codes, 
        IRepository<UserRole> userRoles, IRepository<RefreshToken> refreshTokens) : IUnitOfWork
    {
        public IRepository<User> Users { get; set; } = users;
        public IRepository<Role> Roles { get; set; } = roles;
        public IRepository<UserRole> UserRoles { get; set; } = userRoles;
        public IRepository<ConfirmationCode> ConfirmationCodes { get; set; } = codes;
        public IRepository<RefreshToken> RefreshTokens { get; set; } = refreshTokens;

        public Task<int> SaveChangesAsync()
        {
            return dbContext.SaveChangesAsync();
        }
    }
}