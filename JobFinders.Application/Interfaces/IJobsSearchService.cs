using JobFinders.Application.Models;
using JobFinders.Domain.Models;

using System.Collections.Concurrent;

namespace JobFinders.Application.Interfaces
{
    public interface IJobsSearchService
    {
        Task<JobsSearchResult> SearchAsync(JobsRequest? request, ConcurrentBag<Job> responseList, CancellationToken cancellationToken = default);
    }
}
