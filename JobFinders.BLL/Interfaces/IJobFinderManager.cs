using JobFinders.BLL.Models;

namespace JobFinders.BLL.Interfaces
{
    public interface IJobFinderManager
    {
        Task<IEnumerable<Job?>> ProcessAsync(JobFinderSetting? setting, JobsQuery? filter, CancellationToken cancellationToken = default);
    }
}