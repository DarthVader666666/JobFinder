using JobFinders.BLL.Interfaces;
using JobFinders.BLL.Models;

namespace JobFinders.BLL.Services
{
    public class JobFinderManager: IJobFinderManager
    {        
        private readonly IHtmlLoader _htmlLoader;

        public JobFinderManager(IHtmlLoader htmlLoader)
        {
            _htmlLoader = htmlLoader;
        }

        public async Task<IEnumerable<Job?>> ProcessAsync(JobFinderSetting? setting, JobsQuery? query, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (setting == null)
            {
                throw new Exception($"{nameof(JobFinderSetting)} not found");
            }

            var jobs = (await _htmlLoader.GetJobsAsync(setting, query))
                .Where(job => !(job.Experience is null && job.Location is null && job.Company is null && job.TimePosted is null));               

            return jobs ?? [];
        }
    }
}
