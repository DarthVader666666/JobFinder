namespace JobFinders.Domain.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity? Get(object value);
        IQueryable<TEntity> GetAll();
        Task<TEntity> CreateAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<TEntity> RemoveAsync(TEntity entity);
    }
}
