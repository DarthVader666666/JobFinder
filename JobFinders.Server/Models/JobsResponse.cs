using JobFinders.BLL.Models;
using JobFinders.BLL.Services;

namespace JobFinders.Server.Models
{
    public class JobsResponse
    {
        public Job[][]? JobGroups { get; set; }
        public PageObserver? PageObserver { get; set; } = new PageObserver();
        public bool HasMoreJobs { get; set; } = false;
    }
}