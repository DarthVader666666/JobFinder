using JobFinders.Application.Services;
using JobFinders.Domain.Models;

namespace JobFinders.Application.Models
{
    public class JobsSearchResult
    {
        public Job[][]? JobGroups { get; set; }
        public PageObserver? PageObserver { get; set; } = new PageObserver();
        public bool HasMoreJobs { get; set; } = false;
    }
}