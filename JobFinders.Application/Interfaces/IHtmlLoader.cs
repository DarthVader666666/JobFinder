using JobFinders.Application.Models;
using JobFinders.Domain.Models;

namespace JobFinders.Application.Interfaces
{
    public interface IHtmlLoader
    {
        Task<IEnumerable<Job>> GetJobsAsync(JobFinderSetting? setting, JobsQuery? filter);
    }
}
