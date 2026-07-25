using System.Collections.Concurrent;

using JobFinders.Bll.Models;
using JobFinders.Bll.Services;
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
        private readonly Dictionary<string, string> currencies = new() { ["$"] = "USD", ["€"] = "EUR", ["₽"] = "RUB", ["BYN"] = "BYN" };

        public JobsController(JobFinderManager jobFinderManager, IOptions<List<JobFinderSetting>> jobFinderSettings)
        {
            _jobFinderManager = jobFinderManager;
            _jobFinderSettings = jobFinderSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> GetJobs([FromBody] JobsRequest? request)
        {
            if (request is null)
            {
                return BadRequest();
            }

            var responseList = new ConcurrentBag<Job?>();

            await Parallel.ForEachAsync(request?.Sources ?? [], async (source, ct) =>
            {
                var setting = _jobFinderSettings.FirstOrDefault(x => x.Source == source);
                var filter = new JobsFilter 
                { 
                    ExactTitle = request?.Filter?.ExactTitle ?? false,
                    SalaryDefined = request?.Filter?.SalaryDefined ?? false,
                };

                var jobs = await _jobFinderManager.ProcessAsync(request?.Speciality ?? "", request?.Location ?? "", setting, filter);

                Parallel.ForEach(jobs, (job) =>
                {
                    responseList.Add(job);
                });
            });

            var response = request?.Filter?.OrderBySalary ?? false
                ? responseList.OrderByDescending(x => x.Salary?.Max).AsEnumerable()
                : responseList;

            response = request?.Filter?.Salary?.Currency != "Нет"
                ? response.Select(job => Convert(job, request?.Filter)) : response;

            return Ok(response);
        }

        private Job? Convert(Job? job, Filter? filter)
        {
            if (job?.Salary is null || job.Salary?.Currency == filter?.Salary?.Currency) {
                return job;
            }

            var jobCurrencyData = filter?.CurrencyRates?.FirstOrDefault(rate => rate.Abbreviation == currencies[job?.Salary?.Currency]);
            var apiCurrencyData = filter?.CurrencyRates?.FirstOrDefault(rate => rate.Abbreviation == currencies[filter?.Salary?.Currency]);

            var jobRate = jobCurrencyData?.Rate / jobCurrencyData?.Scale;
            var convertRate = apiCurrencyData?.Rate / apiCurrencyData?.Scale;

            var rate = jobRate / convertRate;

            job?.Salary?.Min = (int?)(job.Salary.Min * rate);
            job?.Salary?.Max = (int?)(job.Salary.Max * rate);
            job?.Salary?.Currency = filter?.Salary?.Currency;

            return job;
        }
    }
}