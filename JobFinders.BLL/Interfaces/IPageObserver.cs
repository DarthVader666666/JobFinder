using JobFinders.BLL.Models;

namespace JobFinders.BLL.Interfaces
{
    public interface IPageObserver
    {
        Task UpdateCounterAsync(PageCounterQuery query);
        PageCounter? InitializeCounter(PageCounterQuery query);
    }
}
