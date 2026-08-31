using JobFinders.Domain.Entities;
using JobFinders.Domain.Interfaces;

namespace JobFinders.DAL.Repositories
{
    public class Repository<TEntity>(JobFinderDbContext dbContext): IRepository<TEntity> where TEntity : class
    {
        public TEntity? Get(object value)
        {
            TEntity? entity = value switch            
            {
                string email when typeof(TEntity) == typeof(User) => dbContext.Users.FirstOrDefault(u => u.Email == email) as TEntity,
                int userId when typeof(TEntity) == typeof(ConfirmationCode) => dbContext.ConfirmationCodes.FirstOrDefault(c => c.UserId == userId) as TEntity,
                _ => null
            };

            return entity;
        }
        
        public IQueryable<TEntity> GetAll()
        {
            return dbContext.Set<TEntity>();
        }
        
        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            var newEntity =  await dbContext.AddAsync(entity);
            
            return newEntity.Entity;
        }
        
        public Task<TEntity> UpdateAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            dbContext.Update(entity);
            
            return Task.FromResult(entity);
        }
        
        public Task<TEntity> RemoveAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            dbContext.Remove(entity);

            return Task.FromResult(entity);
        }
    }
}
