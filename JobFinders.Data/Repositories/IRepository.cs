namespace JobFinders.Data.Repositories
{
    public interface IRepository<T>
    {
        Task<List<T>> GetAllAsync();
    }
}
