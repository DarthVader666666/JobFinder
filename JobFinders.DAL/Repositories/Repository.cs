using JobFinders.Domain.Interfaces;

namespace JobFinders.DAL.Repositories
{
    public class Repository<TEntity>(JobFinderDbContext dbContext): IRepository<TEntity> where TEntity : class
    {
         public IQueryable<TEntity> GetAll()
         {
             return dbContext.Set<TEntity>();
         }

         public Task<int> SaveChangesAsync()
         {
             return dbContext.SaveChangesAsync();
         }

         public async Task<TEntity> CreateAsync(TEntity entity)
         {
             ArgumentNullException.ThrowIfNull(entity);

             await dbContext.AddAsync(entity);

             return entity;
         }

         public TEntity Update(TEntity entity)
         {
             ArgumentNullException.ThrowIfNull(entity);

             dbContext.Update(entity);

             return entity;
         }

         public TEntity Remove(TEntity entity)
         {
             ArgumentNullException.ThrowIfNull(entity);

             dbContext.Remove(entity);

             return entity;
         }
     }
}
