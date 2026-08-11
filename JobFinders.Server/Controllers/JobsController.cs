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
        private readonly List<JobFinderSetting> _jobFinderSettings;

        public JobsController(IJobFinderManager jobFinderManager, IMemoryCache cache, IMapper automapper, IOptions<List<JobFinderSetting>> jobFinderSettings)
        {
            _jobFinderManager = jobFinderManager;
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

            var key = $"{request.Speciality}{request.Location}{string.Join('_', request?.Sources ?? [])}".ToUpper();

            if (_cache.TryGetValue(key, out Job?[][]? cachedJobs))
            { 
                return Ok(cachedJobs);
            }

            var parallelOptions = new ParallelOptions { CancellationToken = cancellationToken };
            var responseList = new ConcurrentBag<Job?>();

            await Parallel.ForEachAsync(request?.Sources ?? [], parallelOptions, async (source, ct) =>
            {
                var setting = _jobFinderSettings.FirstOrDefault(x => x.Source == source);
                var query = _automapper.Map<JobsRequest?, JobsQuery>(request);

                var jobs = await _jobFinderManager.ProcessAsync(setting, query, ct);

                foreach (var job in jobs)
                {
                    responseList.Add(job);
                }
            });

            var groupedResponse = responseList
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
                }, new CompanyComparer())
                .Select(group => group.OrderBy(job => job?.Source).ToArray())
                .ToArray();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .SetPriority(CacheItemPriority.Normal);

            _cache.Set(key, groupedResponse, cacheOptions);

            return Ok(groupedResponse);
        }
    }
}