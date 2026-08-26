using JobFinders.Domain.Models;

using System.Collections.Concurrent;

namespace JobFinders.Domain.Interfaces
{
    public interface IPageObserver
    {
        Task UpdateCounterAsync(PageCounterQuery? query);
        PageCounter? InitializeCounter(PageCounterQuery query);
        void Reset();
        void Set(IPageObserver? pageObserver, string? speciality, string? location);
        bool HasMoreJobs { get; }
        ConcurrentDictionary<string, PageCounter> Counters { get; set; }
    }
}
