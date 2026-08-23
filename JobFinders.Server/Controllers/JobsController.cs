using System.Collections.Concurrent;

using JobFinders.BLL.Models;
using JobFinders.Server.Models;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using JobFinders.BLL.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using AutoMapper;
using JobFinders.Server.Services;

namespace JobFinders.Server.Controllers
{
    [EnableCors("AllowClient")]
    public class JobsController : Controller
    {
        private readonly IJobFinderManager _jobFinderManager;
        private readonly IMemoryCache _cache;
        private readonly IMapper _automapper;
        private readonly IPageObserver _pageObserver;
        private readonly List<JobFinderSetting> _jobFinderSettings;

        public JobsController(IJobFinderManager jobFinderManager, IPageObserver pageObserver, IMemoryCache cache, IMapper automapper, IOptions<List<JobFinderSetting>> jobFinderSettings)
        {
            _jobFinderManager = jobFinderManager;
            _pageObserver = pageObserver;
            _cache = cache;
            _automapper = automapper;
            _jobFinderSettings = jobFinderSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> GetJobs([FromBody] JobsRequest? request, CancellationToken cancellationToken)
        {
            if (request is null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            if(!request.MoreJobs)
            {
                _pageObserver.Reset();
            }

            var key = $"{request.Speciality}{request.Location}{string.Join('_', request?.Sources ?? [])}".ToUpper();

            var responseList = new ConcurrentBag<Job>();

            if (_cache.TryGetValue(key, out JobsResponse? cachedResponse))
            {
                if (cachedResponse?.HasMoreJobs ?? false)
                {
                    if (_pageObserver.Counters.IsEmpty)
                    {
                        _pageObserver.Set(cachedResponse?.PageObserver, request?.Speciality, request?.Location);
                    }

                    foreach (var jobGroup in cachedResponse?.JobGroups ?? [])
                    {
                        Array.ForEach(jobGroup, responseList.Add);
                    }
                }

                if (!request?.MoreJobs ?? false)
                {
                    return Ok(cachedResponse);
                }
            }

            var parallelOptions = new ParallelOptions { CancellationToken = cancellationToken };

            await Parallel.ForEachAsync(request?.Sources ?? [], parallelOptions, async (source, ct) =>
            {
                var setting = _jobFinderSettings.FirstOrDefault(x => x.Source == source);
                var query = _automapper.Map<JobsRequest?, JobsQuery>(request);

                var jobs = await _jobFinderManager.ProcessAsync(setting, query, ct);

                foreach (var job in jobs)
                {
                    var maxIndex = responseList.IsEmpty ? 0 : responseList.Max(job => job.Index);
                    job.Index = maxIndex + 1;
                    responseList.Add(job);
                }
            });

            var response = new JobsResponse
            {
                JobGroups = responseList
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
                    .Select(group => group.OrderBy(job => job?.Source).ToArray())
                    .ToArray(),

                HasMoreJobs = _pageObserver.HasMoreJobs
            };

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .SetPriority(CacheItemPriority.Normal);

            response?.PageObserver?.Counters = new(_pageObserver.Counters);

            _cache.Set(key, response, cacheOptions);

            return Ok(response);
        }
    }
}