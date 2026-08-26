using JobFinders.Domain.Models;

namespace JobFinders.Domain.Interfaces
{
    public interface IHtmlLoader
    {
        Task<IEnumerable<Job>> GetJobsAsync(JobFinderSetting? setting, JobsQuery? filter);
    }
}
