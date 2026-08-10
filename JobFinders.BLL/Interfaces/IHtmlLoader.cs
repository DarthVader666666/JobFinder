using JobFinders.BLL.Models;

namespace JobFinders.BLL.Interfaces
{
    public interface IHtmlLoader
    {
        Task<IEnumerable<Job>> GetJobsAsync(JobFinderSetting? setting, JobsQuery? filter);
    }
}
