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
        private readonly IPageObserver _pageObserver;

        public HtmlLoader(ITransliterator transliterator, IJobParser jobParser, IPageObserver pageObserver)
        {
            _transliterator = transliterator;
            _jobParser = jobParser;
            _pageObserver = pageObserver;
        }

        public async Task<IEnumerable<Job>> GetJobsAsync(JobFinderSetting? setting, JobsQuery? query)
        {
            ArgumentNullException.ThrowIfNull(setting);

            var pageCounterQuery = new PageCounterQuery(setting?.Source, query?.Speciality, query?.Location);
            var counter = _pageObserver?.InitializeCounter(pageCounterQuery);
            //var currentPage = setting?.ZeroBasedPagination ?? false ? counter?.CurrentPage : counter?.CurrentPage + 1;
            var currentPage = setting?.ZeroBasedPagination ?? false ? 0 : 1;

            query?.Location = _transliterator.Transliterate(query?.Location, setting);

            var url = setting?.LinkTemplate?.Replace(locationPlaceholder, query?.Location)
                .Replace(specialityPlaceholder, WebUtility.UrlEncode(query?.Speciality))
                .Replace(pagePlaceholder, $"{currentPage}");

            var vacancyNodes = Enumerable.Empty<HtmlNode>();
            IEnumerable<Job> jobs;

            try
            {
                var htmlDoc = await new HtmlWeb().LoadFromWebAsync(url ?? "");

                vacancyNodes = GetDescendantNodes(htmlDoc?.DocumentNode, setting?.VacancyTag);
                var navigationNodes = GetDescendantNodes(htmlDoc?.DocumentNode, setting?.NavigationTag);

                pageCounterQuery.CurrentPage = (counter?.CurrentPage ?? 0) + 1;

                _ = ProcessPages(navigationNodes, pageCounterQuery);
            }
            catch (Exception ex)
            {
                return [new Job { Title = $"{setting?.Source} Error: {ex.Message}", Logo = new() { Source = setting?.Source, Url = url } }];
            }

            jobs = JobsIterator(setting, vacancyNodes, url).DistinctBy(x => x.Link);

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

        private async Task ProcessPages(IEnumerable<HtmlNode>? navigationNodes, PageCounterQuery? query) 
        {
            if (!(navigationNodes ?? []).Any())
            {
                return;
            }

            var anchors = navigationNodes?.First().Descendants("a") ?? [];
            var hasNextPage = anchors.Any(node => int.TryParse(node.InnerText, out int pageNumber) && pageNumber > query?.CurrentPage);

            query?.HasNextPage = hasNextPage;

            _ = _pageObserver.UpdateCounterAsync(query);
        }

        private IEnumerable<HtmlNode> GetDescendantNodes(HtmlNode? coreNode, HtmlTag? tag)
        {
            if (coreNode is null || tag is null)
            {
                return [];
            }

            return coreNode.Descendants(tag.Tag ?? "")
                    .Where(n => n?.Attributes[tag.HtmlAttribute?.Attribute ?? ""] != null
                        && n.Attributes[tag.HtmlAttribute?.Attribute ?? ""].Value.Contains($"{tag.HtmlAttribute?.Value}")) ?? [];
        }
    }
}
