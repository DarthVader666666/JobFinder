namespace JobFinders.Domain.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetAll();
        Task<TEntity> CreateAsync(TEntity entity);
        TEntity Update(TEntity entity);
        TEntity Remove(TEntity entity);
    }
}
