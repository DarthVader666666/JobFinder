using System.Net;

using HtmlAgilityPack;

using JobFinders.Domain.Interfaces;
using JobFinders.Domain.Models;

namespace JobFinders.Application.Services
{
    public partial class HtmlLoader: IHtmlLoader
    {
        private const string locationPlaceholder = "*location*";
        private const string specialityPlaceholder = "*speciality*";
        private const string pagePlaceholder = "*page*";

        private readonly ITransliterator _transliterator;
        private readonly IPageObserver _pageObserver;

        private static readonly HttpClient HttpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var handler = new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
                ConnectTimeout = TimeSpan.FromSeconds(20),
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            };

            var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            return client;
        }

        public HtmlLoader(ITransliterator transliterator, IPageObserver pageObserver)
        {
            _transliterator = transliterator;
            _pageObserver = pageObserver;

            currencies = usd.Concat(euro).Concat(belRub).Concat(rusRub).Concat(tenge).Concat(lari).Concat(manat).Concat(som).ToArray();
        }

        public async Task<IEnumerable<Job>> GetJobsAsync(JobFinderSetting? setting, JobsQuery? query)
        {
            ArgumentNullException.ThrowIfNull(setting);

            var pageCounterQuery = new PageCounterQuery(setting?.Source, query?.Speciality, query?.Location);
            var counter = _pageObserver?.InitializeCounter(pageCounterQuery);

            if (counter?.HasNextPage is not null && !(bool)counter.HasNextPage)
            {
                return [];
            }

            var currentPage = setting?.ZeroBasedPagination ?? false ? counter?.CurrentPage : counter?.CurrentPage + 1;

            query?.Location = _transliterator.Transliterate(query?.Location, setting);

            var url = setting?.LinkTemplate?.Replace(locationPlaceholder, query?.Location)
                .Replace(specialityPlaceholder, WebUtility.UrlEncode(query?.Speciality))
                .Replace(pagePlaceholder, $"{currentPage}");

            var vacancyNodes = Enumerable.Empty<HtmlNode>();
            IEnumerable<Job> jobs;

            try
            {
                var htmlDoc = new HtmlDocument();

                using (var loadTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(3)))
                using (var stream = await HttpClient.GetStreamAsync(url ?? "", loadTimeout.Token))
                {
                    htmlDoc.Load(stream);
                }

                vacancyNodes = GetDescendantNodes(htmlDoc?.DocumentNode, setting?.VacancyTag);
                var navigationNodes = GetDescendantNodes(htmlDoc?.DocumentNode, setting?.NavigationTag);

                pageCounterQuery.CurrentPage = (counter?.CurrentPage ?? 0) + 1;

                _ = ProcessPages(navigationNodes, pageCounterQuery);
            }
            catch (Exception ex)
            {
                return [new Job 
                { 
                    Title = $"{setting?.Source} Error: {GetErrorMessage(ex.Message)}", 
                    Source = setting?.Source, 
                    Logo = new() { Source = setting?.Source, Url = url }, 
                    Experience = "", 
                    Location = "", 
                    TimePosted = "", 
                    Company = "", 
                    Link = url
                }];
            }

            jobs = JobsIterator(setting, vacancyNodes, url).DistinctBy(x => x.Link);

            return jobs;
        }

        private static string GetErrorMessage(string fullMessage)
        {
            return fullMessage switch
            {
                var x when x.Contains("403") => "Ресурс запретил приложению доступ.",
                _ => fullMessage
            };
        }

        private IEnumerable<Job> JobsIterator(JobFinderSetting? setting, IEnumerable<HtmlNode> nodes, string? url)
        {
            foreach (var node in nodes)
            {
                var job = Parse(setting, node, url);

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
                query?.HasNextPage = false;
                _ = _pageObserver.UpdateCounterAsync(query);
                return;
            }

            var anchors = navigationNodes?.First().Descendants("a") ?? [];
            var hasNextPage = anchors.Any(node => int.TryParse(node.InnerText, out int pageNumber) && pageNumber > query?.CurrentPage);

            query?.HasNextPage = hasNextPage;
            _ = _pageObserver.UpdateCounterAsync(query);
        }

        private IEnumerable<HtmlNode> GetDescendantNodes(HtmlNode? coreNode, JobFinderTag? tag)
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
