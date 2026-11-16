
namespace JobFinders.Domain.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity? GetBy(object value);
        IQueryable<TEntity> All { get; }
        Task<TEntity> CreateAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<TEntity> RemoveAsync(TEntity entity);
    }
}
