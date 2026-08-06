using System.Collections.Concurrent;

using JobFinders.BLL.Services;
using JobFinders.BLL.Models;
using JobFinders.Server.Models;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JobFinders.Server.Controllers
{
    [EnableCors("AllowClient")]
    public class JobsController : Controller
    {
        private readonly JobFinderManager _jobFinderManager;
        private readonly List<JobFinderSetting> _jobFinderSettings;

        public JobsController(JobFinderManager jobFinderManager, IOptions<List<JobFinderSetting>> jobFinderSettings)
        {
            _jobFinderManager = jobFinderManager;
            _jobFinderSettings = jobFinderSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> GetJobs([FromBody] JobsRequest? request, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                return BadRequest();
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
                        SalaryDefined = request?.Filter?.SalaryDefined ?? false,
                        Salary = new()
                        {
                            Currency = request?.Filter?.Salary?.Currency,
                            Min = request?.Filter?.Salary?.Min,
                            Max = request?.Filter?.Salary?.Max
                        },
                        CurrencyRates = request?.Filter?.CurrencyRates
                    };

                    var jobs = await _jobFinderManager.ProcessAsync(setting, filter);

                    Parallel.ForEach(jobs, (job) =>
                    {
                        responseList.Add(job);
                    });
                });
            }
            catch (Exception ex) 
            {
                throw ex;
            }            

            var response = request?.Filter?.OrderBySalary ?? false
                ? responseList.OrderByDescending(x => x?.Salary?.Max).AsEnumerable()
                : responseList;

            response = request?.Filter?.GroupBySource ?? false
                ? response.OrderBy(x => x?.Source).AsEnumerable()
                : response;

            return Ok(response);
        }
    }
}