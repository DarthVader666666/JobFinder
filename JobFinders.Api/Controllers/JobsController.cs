using System.Collections.Concurrent;

using JobFinders.Api.Models;

using MapsterMapper;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

using JobFinders.Domain.Interfaces;
using JobFinders.Domain.Models;
using JobFinders.Domain.Comparers;
using JobFinders.Api.Attributes;
using JobFinders.Application.Models;
using JobFinders.Application.Interfaces;

namespace JobFinders.Api.Controllers
{
    [EnableCors("AllowClient")]
    public class JobsController : Controller
    {
        private readonly IJobsSearchService _jobsSearchService;
        private readonly IMemoryCache _cache;
        private readonly IPageObserver _pageObserver;

        public JobsController(IJobFinderManager jobFinderManager, IJobsSearchService jobsSearchService, IPageObserver pageObserver, IMemoryCache cache, IOptions<List<JobFinderSetting>> jobFinderSettings)
        {
            _jobsSearchService = jobsSearchService;
            _pageObserver = pageObserver;
            _cache = cache;
        }

        [InterceptRequest]
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

            if (_cache.TryGetValue(key, out JobsSearchResult? cachedResponse))
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

            var response = await _jobsSearchService.SearchAsync(request, responseList, cancellationToken);

            response.HasMoreJobs = _pageObserver.HasMoreJobs;

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