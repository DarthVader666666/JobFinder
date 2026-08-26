using JobFinders.Application.Services;
using JobFinders.Domain.Models;

namespace JobFinders.Api.Models
{
    public class JobsResponse
    {
        public Job[][]? JobGroups { get; set; }
        public PageObserver? PageObserver { get; set; } = new PageObserver();
        public bool HasMoreJobs { get; set; } = false;
    }
}