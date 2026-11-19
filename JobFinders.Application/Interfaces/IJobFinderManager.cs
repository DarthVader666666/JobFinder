using JobFinders.Application.Models;
using JobFinders.Domain.Models;

namespace JobFinders.Application.Interfaces
{
    public interface IJobFinderManager
    {
        Task<IEnumerable<Job>> ProcessAsync(JobFinderSetting? setting, JobsQuery? filter, CancellationToken cancellationToken = default);
    }
}