using JobFinders.BLL.Interfaces;
using JobFinders.BLL.Models;

namespace JobFinders.BLL.Services
{
    public class JobFinderManager: IJobFinderManager
    {        
        private readonly ICurrencyConverter _currencyConverter;
        private readonly IHtmlLoader _htmlLoader;

        public JobFinderManager(IHtmlLoader htmlLoader, ICurrencyConverter currencyConverter)
        {
            _currencyConverter = currencyConverter;
            _htmlLoader = htmlLoader;
        }

        public async Task<IEnumerable<Job?>> ProcessAsync(JobFinderSetting? setting, JobsFilter? filter, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (setting == null)
            {
                throw new Exception($"{nameof(JobFinderSetting)} not found");
            }

            var jobs = (await _htmlLoader.GetJobsAsync(setting, filter))
                .Where(job => 
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    return !(job.Experience is null && job.Location is null && job.Company is null && job.TimePosted is null);
                })
                .Where(job =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (filter?.ExactTitle ?? false)
                    {
                        var specialityParts = filter?.Speciality?.Split([' ', '-']) ?? [];
                        var titleParts = job.Title?.Split([' ', '-']) ?? [];

                        return specialityParts.Any(sp => titleParts.Any(tp => tp.Contains(sp.Trim(), StringComparison.InvariantCultureIgnoreCase)));
                    }

                    return true;
                })
                .Select(job => 
                {
                    job.Salary = _currencyConverter.Convert(job.Salary, filter);
                    return job;
                })
                .Where(job =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (filter?.Salary?.Min > 0)
                    {
                        return !string.IsNullOrEmpty(job?.Salary?.Currency)
                            && (job.Salary.Min != job.Salary.Max || job.Salary.Min >= filter?.Salary?.Min)
                            && job?.Salary.Min <= filter?.Salary?.Max
                            && job?.Salary.Max >= filter.Salary.Min
                            && (job.Salary.Min <= filter.Salary.Max || job.Salary.Max <= filter.Salary.Max);
                    }
                    else
                    {
                        return job?.Salary is null || job.Salary?.Max <= filter?.Salary?.Max;
                    }
                });

            return jobs ?? [];
        }
    }
}
