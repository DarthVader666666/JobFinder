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
                Counters?.Add(query.Source, counter);
            }    

            return Counters!.GetValueOrDefault(query.Source);
        }

        public async Task UpdateCounterAsync(PageCounterQuery query)
        {
            if (Counters.TryGetValue(query.Source ?? "", out PageCounter? counter))
            {
                if (query.HasNextPage)
                {
                    counter.CurrentPage++;
                }

                if (!query.HasNextPage && !counter.HasNextPage)
                {
                    Counters?.Remove(query?.Source);
                }
            }
            else
            {
                var newCounter = new PageCounter(query.Source)
                {
                    HasNextPage = query.HasNextPage
                };

                Counters?.Add(query.Source, newCounter);
            }
        }

        private void ResetCounter(string source) 
        {
            var counter = Counters[source];

            counter?.CurrentPage = 0;
            counter?.HasNextPage = false;
        }

        private Dictionary<string, PageCounter> Counters { get; set; } = new();
        private string? Speciality { get; set; }
        private string? Location { get; set; }
    }
}
