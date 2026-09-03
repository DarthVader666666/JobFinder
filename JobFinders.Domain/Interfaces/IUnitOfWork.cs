using JobFinders.Domain.Entities;

namespace JobFinders.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<User> Users { get; set; }
        IRepository<Role> Roles { get; set; }
        IRepository<UserRole> UserRoles { get; set; }
        IRepository<ConfirmationCode> ConfirmationCodes { get; set; }
        Task<int> SaveChangesAsync();
    }
}
