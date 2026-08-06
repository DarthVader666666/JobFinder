using System.Net;

using HtmlAgilityPack;

using JobFinders.BLL.Interfaces;
using JobFinders.BLL.Models;

namespace JobFinders.BLL.Services
{
    public class HtmlLoader: IHtmlLoader
    {
        private const string locationPlaceholder = "*location*";
        private const string specialityPlaceholder = "*speciality*";
        private const string pagePlaceholder = "*page*";

        private readonly ITransliterator _transliterator;
        private readonly IJobParser _jobParser;

        public HtmlLoader(ITransliterator transliterator, IJobParser jobParser)
        {
            _transliterator = transliterator;
            _jobParser = jobParser;
        }

        public async Task<IEnumerable<Job>> GetJobsAsync(JobFinderSetting? setting, JobsFilter? filter)
        {
            filter?.Location = _transliterator.Transliterate(filter?.Location, setting);
            var url = setting?.LinkTemplate?.Replace(locationPlaceholder, filter?.Location).Replace(specialityPlaceholder, WebUtility.UrlEncode(filter?.Speciality));

            var nodes = Enumerable.Empty<HtmlNode>();
            IEnumerable<Job> jobs;

            try
            {                
                var htmlDoc = await new HtmlWeb().LoadFromWebAsync(url);

                nodes = (htmlDoc?.DocumentNode?.Descendants(setting?.VacancyTag?.Tag ?? "")
                    .Where(n => n?.Attributes["class"] != null && n.Attributes["class"].Value.Contains($"{setting?.VacancyTag?.HtmlAttribute?.Value}")) ?? []);
            }
            catch (Exception ex)
            {
                return [new Job { Title = $"{setting?.Source} Error: {ex.Message}", Logo = new() { Source = setting?.Source, Url = url } }];
            }

            jobs = JobsIterator(setting, nodes, url).DistinctBy(x => x.Link);

            return jobs;
        }

        private IEnumerable<Job> JobsIterator(JobFinderSetting? setting, IEnumerable<HtmlNode> nodes, string? url)
        {
            foreach (var node in nodes)
            {
                var job = _jobParser.Parse(setting, node, url);

                if (job is not null)
                { 
                    yield return job;
                }
            }
        }
    }
}
