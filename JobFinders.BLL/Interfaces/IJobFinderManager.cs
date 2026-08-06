using JobFinders.BLL.Models;

namespace JobFinders.BLL.Interfaces
{
    public interface IJobFinderManager
    {
        Task<IEnumerable<Job?>> ProcessAsync(JobFinderSetting? setting, JobsFilter? filter, CancellationToken cancellationToken = default);
    }
}