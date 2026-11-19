using JobFinders.Application.Interfaces;
using JobFinders.Application.Models;
using JobFinders.Domain.Comparers;
using JobFinders.Domain.Models;

using MapsterMapper;

using Microsoft.Extensions.Options;

using System.Collections.Concurrent;

namespace JobFinders.Application.Services
{
    public class JobsSearchService : IJobsSearchService
    {
        private readonly IJobFinderManager _jobFinderManager;
        private readonly IReadOnlyList<JobFinderSetting> _jobFinderSettings;
        private readonly IMapper _mapster;

        public JobsSearchService(
            IJobFinderManager jobFinderManager,
            IOptions<List<JobFinderSetting>> jobFinderSettings,
            IMapper mapster)
        {
            _jobFinderManager = jobFinderManager;
            _jobFinderSettings = jobFinderSettings.Value;
            _mapster = mapster;
        }

        public async Task<JobsSearchResult> SearchAsync(JobsRequest? request, ConcurrentBag<Job> responseList, CancellationToken cancellationToken = default)
        {
            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Math.Max(1, request?.Sources?.Length ?? 1),
            };

            await Parallel.ForEachAsync(request?.Sources ?? [], parallelOptions, async (source, ct) =>
            {
                var setting = _jobFinderSettings.FirstOrDefault(x => x.Source == source);
                var query = _mapster.Map<JobsRequest?, JobsQuery>(request);

                var jobs = await _jobFinderManager.ProcessAsync(setting, query, ct);

                foreach (var job in jobs)
                {
                    var maxIndex = responseList.IsEmpty ? 0 : responseList.Max(job => job.Index);
                    job.Index = maxIndex + 1;
                    responseList.Add(job);
                }
            });

            var result = new JobsSearchResult
            {
                JobGroups = [.. responseList
                    .DistinctBy(job => job.Link?.ToUpper())
                    .OrderBy(job => job, new TimePostedComparer())
                    .GroupBy(job => new Job
                    {
                        Title = job?.Title,
                        OriginalSalary = new Salary
                        {
                            Currency = job?.OriginalSalary?.Currency,
                            Min = job?.OriginalSalary?.Min,
                            Max = job?.OriginalSalary?.Max,
                        },
                        Company = job?.Company
                    }, new JobEqualityComparer())
                    .Select(group => group.OrderBy(job => job?.Source).ToArray())],
            };

            return result;
        }
    }
}
