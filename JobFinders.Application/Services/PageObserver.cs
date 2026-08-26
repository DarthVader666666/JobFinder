using System.Collections.Concurrent;

using JobFinders.Domain.Interfaces;
using JobFinders.Domain.Models;

namespace JobFinders.Application.Services
{
    public class PageObserver: IPageObserver
    {
        public PageCounter? InitializeCounter(PageCounterQuery query)
        {
            if (string.IsNullOrEmpty(query.Source))
            {
                return null;
            }

            if (Counters.TryGetValue(query.Source, out PageCounter? counter))
            {
                if (query.Speciality?.ToUpper() != Speciality || query.Location?.ToUpper() != Location)
                {
                    ResetCounter(query.Source);
                }
            }
            else
            {
                counter = new PageCounter(query.Source);
                Speciality = query.Speciality?.ToUpper();
                Location = query.Location?.ToUpper();
                _ = Counters?.TryAdd(query.Source, counter);
            }

            return Counters!.GetValueOrDefault(query.Source);
        }

        public async Task UpdateCounterAsync(PageCounterQuery? query)
        {
            if (string.IsNullOrEmpty(query?.Source))
            {
                return;
            }

            if (Counters.TryGetValue(query.Source, out PageCounter? counter))
            {
                counter.HasNextPage = query.HasNextPage;

                if (counter.HasNextPage ?? false)
                {
                    counter.CurrentPage++;
                }
            }
            else
            {
                var newCounter = new PageCounter(query.Source)
                {
                    HasNextPage = query.HasNextPage
                };

                _ = Counters?.TryAdd(query.Source, newCounter);
            }
        }

        public void Reset()
        {
            Counters = new();
            Speciality = null;
            Location = null;
        }

        public void Set(IPageObserver? pageObserver, string? speciality, string? location)
        {
            Counters = new(pageObserver?.Counters ?? []);
            Speciality = speciality?.ToUpper();
            Location = location?.ToUpper();
        }

        private void ResetCounter(string source) 
        {
            var counter = Counters[source];

            counter?.CurrentPage = 0;
            counter?.HasNextPage = null;
        }

        public bool HasMoreJobs => Counters.Any(c => c.Value.HasNextPage ?? false);
        public ConcurrentDictionary<string, PageCounter> Counters { get; set; } = new();
        private string? Speciality { get; set; }
        private string? Location { get; set; }
    }
}
