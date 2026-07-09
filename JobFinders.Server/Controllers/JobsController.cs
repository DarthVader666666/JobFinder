using System.Collections.Concurrent;

using JobFinders.Bll.Models;
using JobFinders.Bll.Services;
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
        public async Task<IActionResult> GetJobs([FromBody] JobsRequest request)
        {
            var responseList = new ConcurrentBag<Job>();

            await Parallel.ForEachAsync(request.Sources, async (source, ct) =>
            {
                var setting = _jobFinderSettings.FirstOrDefault(x => x.Source == source);
                var jobs = await _jobFinderManager.ProcessAsync(request.Speciality, request.Location, setting);

                foreach (var job in jobs)
                {
                    responseList.Add(job);
                }
            });            

            return responseList != null ? Ok(responseList) : BadRequest();
        }
    }
}