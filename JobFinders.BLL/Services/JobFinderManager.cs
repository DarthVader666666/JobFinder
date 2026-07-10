using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using JobFinders.Bll.Enums;
using JobFinders.Bll.Models;

using Microsoft.Extensions.Options;

using NickBuhro.Translit;

namespace JobFinders.Bll.Services
{
    public class JobFinderManager
    {
        private const string locationPlaceholder = "*location*";
        private const string specialityPlaceholder = "*speciality*";
        private const string pagePlaceholder = "*page*";

        private readonly string[] usd = { "$", "USD" };
        private readonly string[] euro = { "€", "EUR" };
        private readonly string[] belRub = { "Br", "BYN", "руб" };
        private readonly string[] rusRub = { "₽" };

        private readonly string[] currencies;

        public JobFinderManager()
        {
            currencies = usd.Concat(euro).Concat(belRub).Concat(rusRub).ToArray();
        }

        public async Task<IEnumerable<Job>> ProcessAsync(string? speciality, string? location, JobFinderSetting? setting)
        {
            var transliteration = Enum.Parse<TransliterationEnum>(setting.LocationTransliteration);

            location ??= setting.MandatoryLocation ? "minsk" : string.Empty;

            location = transliteration switch
            {
                TransliterationEnum.Latin => Transliteration.CyrillicToLatin(location),
                TransliterationEnum.Cyrillic => Transliteration.LatinToCyrillic(location),
            };

            speciality ??= string.Empty;

            var url = setting.LinkTemplate?.Replace(locationPlaceholder, location).Replace(specialityPlaceholder, speciality);

            if (setting == null)
            {
                throw new Exception("JobFinderSetting not found");
            }

            var jobs = await GetJobsAsync(speciality, location, url, setting);

            return jobs;
        }

        private async Task<IEnumerable<Job>> GetJobsAsync(string? speciality, string? location, string? url, JobFinderSetting? setting)
        {
            IEnumerable<HtmlNode> nodes;

            try
            {
                var doc1 = await new HtmlWeb().LoadFromWebAsync(url);

                //var client = new HttpClient();
                //client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                //string html = await client.GetStringAsync(url);

                //var doc2 = new HtmlDocument();
                //doc2.LoadHtml(html);

                nodes = (doc1?.DocumentNode?.Descendants(setting.NodeTag)
                    .Where(n => n?.Attributes["class"] != null && n.Attributes["class"].Value.Contains($"{setting.TagCssClass}")) ?? []);
                    //.Concat(doc2?.DocumentNode?.Descendants(setting.NodeTag)
                    //.Where(n => n?.Attributes["class"] != null && n.Attributes["class"].Value.Contains($"{setting.TagCssClass}")) ?? []);
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<Job>().Append(new Job { Title = "Loading html:" + " " + ex.Message });
            }            

            var jobs = Enumerable.Empty<Job>();

            try
            {
                jobs = JobsIterator(setting, nodes, url).DistinctBy(x => x.Link);
            }
            catch (Exception ex)
            {
                jobs = Enumerable.Empty<Job>().Append(new Job { Title = ex.Message });
            }

            return jobs;            
        }

        private IEnumerable<Job> JobsIterator(JobFinderSetting setting, IEnumerable<HtmlNode> nodes, string? url)
        {
            foreach (var node in nodes)
            {
                var anchor = node.Descendants("a").FirstOrDefault(node =>
                    node.Attributes["href"] != null && node.Attributes["href"].Value.Contains(setting.HrefPrefix) && node.InnerText.Trim().Any());

                var href = anchor?.Attributes["href"].Value;

                if (anchor != null)
                {
                    var descendants = node.Descendants();

                    yield return new Job
                    {
                        Link = setting.AddBaseUrlToHrefPrefix ? setting.BaseUrl + href : href,
                        Title = GetTitle(anchor.InnerText),
                        Salary = GetSalary(descendants, setting),
                        Logo = new Logo { Source = setting.Source, Url = url }
                    };
                }
            }
        }


        private string? GetTitle(string title)
        {
            return ConvertSpecialSymbols(title);
        }

        private Salary? GetSalary(IEnumerable<HtmlNode> nodes, JobFinderSetting setting)
        {
            var innerText =  string.IsNullOrEmpty(setting.SalaryCssClass)
                ? nodes.FirstOrDefault(x => ContainsCurrencySymbols(x.InnerText))?.InnerText
                : nodes.FirstOrDefault(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains(setting.SalaryCssClass))?.InnerText;

            if (string.IsNullOrEmpty(innerText))
            { 
                return null;
            }

            var salary = new Salary();

            innerText = ConvertSpecialSymbols(innerText);

            var currencyPattern = $@"(?i){string.Join("|", currencies.Select(Regex.Escape))}";
            var currencyMatch = Regex.Match(innerText, currencyPattern);

            if (currencyMatch.Success)
            {
                int index = currencyMatch.Index;

                int start = Math.Max(0, index - 20);
                int length = Math.Min(innerText.Length - start, 20);

                var substring = innerText.Substring(start, length);

                if (substring.IsWhiteSpace())
                {
                    start = index;
                    innerText = innerText.Substring(start, length);
                }
                else if (Regex.IsMatch(substring, currencyPattern))
                {
                    innerText = substring;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            var predicate = new Predicate<string[]>(currencies => currencies.Any(c => currencyMatch.Value == c));

            salary.Currency = predicate switch
            {
                var x when x(usd) => "$",
                var x when x(euro) => "€",
                var x when x(belRub) => "BYN",
                var x when x(rusRub) => "₽",
                _ => null
            };

            innerText = Regex.Replace(innerText, @"[^\d\s\-–—]", "");
            innerText = Regex.Replace(innerText, @"\s+", "");

            var match = Regex.Match(innerText, @"^(?<min>\d+)(?:[-–—](?<max>\d+))?$");

            if (match.Success)
            {
                var min = match.Groups["min"].Value.Trim();
                var max = match.Groups["max"].Success ? match.Groups["max"].Value.Trim() : min;

                salary.Min = ParseSalary(min);
                salary.Max = ParseSalary(max);
            }
            else
            {
                salary.Min = salary.Max = ParseSalary(innerText);
            }

            return salary;
        }

        private bool ContainsCurrencySymbols(string innerText)
        {
            return currencies.Any(innerText.Contains);
        }

        private static string? ConvertSpecialSymbols(string? innerText)
        {
            return innerText?
                .Replace("&nbsp;", " ")?
                .Replace("&quot;", "\"")?
                .Replace("&amp;", "&")?
                .Replace("&mdash;", "-")?
                .Replace("<!--", "")?
                .Replace("-->", "")
                .Replace("\n", "")
                .Replace("\t", "")
                .Trim();
        }

        private int ParseSalary(string salary)
        {
            salary = Regex.Replace(salary, @"\s+", " ").Trim();
            salary = Regex.Replace(salary, @"(?<=\d)\s+(?=\d)", "");

            return int.Parse(salary);
        }
    }
}
