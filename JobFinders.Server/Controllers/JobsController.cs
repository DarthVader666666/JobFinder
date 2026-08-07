using System.Collections.Concurrent;

using JobFinders.BLL.Models;
using JobFinders.Server.Models;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using JobFinders.BLL.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace JobFinders.Server.Controllers
{
    [EnableCors("AllowClient")]
    public class JobsController : Controller
    {
        private readonly IJobFinderManager _jobFinderManager;
        private readonly IMemoryCache _cache;
        private readonly List<JobFinderSetting> _jobFinderSettings;

        public JobsController(IJobFinderManager jobFinderManager, IMemoryCache cache, IOptions<List<JobFinderSetting>> jobFinderSettings)
        {
            _jobFinderManager = jobFinderManager;
            _cache = cache;
            _jobFinderSettings = jobFinderSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> GetJobs([FromBody] JobsRequest? request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                return BadRequest();
            }

            var key = $"{request.Speciality}{request.Location}".ToUpper();

            if (_cache.TryGetValue(key, out ConcurrentBag<Job?>? cachedJobs))
            { 
                return Ok(cachedJobs);
            }

            var parallelOptions = new ParallelOptions { CancellationToken = cancellationToken };
            var responseList = new ConcurrentBag<Job?>();

            try
            {
                await Parallel.ForEachAsync(request?.Sources ?? [], parallelOptions, async (source, ct) =>
                {
                    var setting = _jobFinderSettings.FirstOrDefault(x => x.Source == source);
                    var filter = new JobsFilter
                    {
                        Speciality = request?.Speciality,
                        Location = request?.Location,
                        ExactTitle = request?.Filter?.ExactTitle ?? false,
                        Salary = new()
                        {
                            Currency = request?.Filter?.Salary?.Currency,
                            Min = request?.Filter?.Salary?.Min,
                            Max = request?.Filter?.Salary?.Max
                        }
                    };

                    var jobs = await _jobFinderManager.ProcessAsync(setting, filter, ct);

                    foreach (var job in jobs)
                    {
                        responseList.Add(job);
                    }
                });
            }
            catch
            {
                throw;
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .SetPriority(CacheItemPriority.Normal);

            _cache.Set(key, responseList, cacheOptions);

            return Ok(responseList);
        }
    }
}