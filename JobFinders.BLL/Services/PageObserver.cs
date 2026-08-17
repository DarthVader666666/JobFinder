using System.Collections.Concurrent;

using JobFinders.BLL.Interfaces;
using JobFinders.BLL.Models;

namespace JobFinders.BLL.Services
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
                if (query.Speciality != Speciality || query.Location != Location)
                {
                    ResetCounter(query.Source);
                }
            }
            else
            {
                counter = new PageCounter(query.Source);
                Speciality = query.Speciality;
                Location = query.Location;
                _ = Counters?.TryAdd(query.Source, counter);
            }

            return Counters!.GetValueOrDefault(query.Source);
        }

        public async Task UpdateCounterAsync(PageCounterQuery query)
        {
            if (Counters.TryGetValue(query.Source ?? "", out PageCounter? counter))
            {
                counter.HasNextPage = query.HasNextPage;

                if (counter.HasNextPage ?? false)
                {
                    counter.CurrentPage++;
                }

                //if (!query.HasNextPage && !counter.HasNextPage)
                //{
                //    _ = Counters?.TryRemove(query?.Source ?? "", out _);
                //}                
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

        private void ResetCounter(string source) 
        {
            var counter = Counters[source];

            counter?.CurrentPage = 0;
            counter?.HasNextPage = null;
        }

        public bool HasMoreJobs => Counters.Any(c => c.Value.HasNextPage ?? false);
        private ConcurrentDictionary<string, PageCounter> Counters { get; set; } = new();
        private string? Speciality { get; set; }
        private string? Location { get; set; }
    }
}
