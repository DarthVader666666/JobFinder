using JobFinders.Domain.Models;

namespace JobFinders.Domain.Interfaces
{
    public interface IJobFinderManager
    {
        Task<IEnumerable<Job>> ProcessAsync(JobFinderSetting? setting, JobsQuery? filter, CancellationToken cancellationToken = default);
    }
}