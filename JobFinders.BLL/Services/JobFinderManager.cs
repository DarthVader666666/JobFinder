using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using JobFinders.BLL.Enums;
using JobFinders.BLL.Models;

using NickBuhro.Translit;

namespace JobFinders.BLL.Services
{
    public class JobFinderManager
    {
        private const string locationPlaceholder = "*location*";
        private const string specialityPlaceholder = "*speciality*";
        private const string pagePlaceholder = "*page*";

        private readonly string[] usd = { "$", "USD" };
        private readonly string[] euro = { "€", "EUR" };
        private readonly string[] belRub = { "Br", "BYN", "руб", "" };
        private readonly string[] rusRub = { "₽" };
        private readonly string[] tenge = { "₸" };
        private readonly string[] lari = { "₾" };
        private readonly string[] manat = { "₼" };
        private readonly string[] som = { "so'm" };

        private readonly Dictionary<string, string> currenciesApi = new() { ["$"] = "USD", ["€"] = "EUR", ["₽"] = "RUB", ["BYN"] = "BYN", ["₸"] = "KZT", ["₾"] = "GEL", ["₼"] = "AZN", ["so'm"] = "UZS", };

        private readonly string[] currencies;

        public JobFinderManager()
        {
            currencies = usd.Concat(euro).Concat(belRub).Concat(rusRub).Concat(tenge).Concat(lari).Concat(manat).Concat(som).ToArray();
        }

        public async Task<IEnumerable<Job?>> ProcessAsync(JobFinderSetting? setting, JobsFilter? filter)
        {
            var transliteration = Enum.Parse<TransliterationEnum>(setting.LocationTransliteration);

            filter?.Location = string.IsNullOrEmpty(filter?.Location) && setting.MandatoryLocation ? "minsk" : filter?.Location;
            filter?.Location = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(filter?.Location ?? "");

            filter?.Location = transliteration switch
            {
                TransliterationEnum.Latin => Transliteration.CyrillicToLatin(filter?.Location),
                TransliterationEnum.Cyrillic => Transliteration.LatinToCyrillic(filter?.Location),
            };

            if (setting.ConvertLocation)
            {
                filter?.Location = setting?.LocationDictionary?.FirstOrDefault(x => filter?.Location?.Contains(x.Key, StringComparison.InvariantCultureIgnoreCase) ?? false).Value ?? string.Empty;
            }

            filter?.Speciality = filter?.Speciality is null ? string.Empty : WebUtility.UrlEncode(filter?.Speciality);

            var url = setting.LinkTemplate?.Replace(locationPlaceholder, filter?.Location).Replace(specialityPlaceholder, filter?.Speciality);

            if (setting == null)
            {
                throw new Exception($"{nameof(JobFinderSetting)} not found");
            }

            var jobs = (await GetJobsAsync(url, setting))
                .Where(job => !(job.Experience is null && job.Location is null && job.Company is null && job.TimePosted is null))
                .Where(job =>
                {
                    if (filter?.ExactTitle ?? false)
                    {
                        var specialityParts = filter?.Speciality?.Split([' ', '-']) ?? [];
                        return specialityParts.Any(s => job.Title?.Contains(s.Trim(), StringComparison.InvariantCultureIgnoreCase) ?? false);
                    }

                    return true;
                })
                .Where(job =>
                {
                    if (filter?.SalaryDefined ?? false)
                    {
                        return !string.IsNullOrEmpty(job?.Salary?.Currency)
                            && job?.Salary.Min >= filter?.Salary?.Min
                            && job?.Salary.Max <= filter.Salary.Max;
                    }

                    return true;
                })
                .Select(job => filter?.Salary?.Currency != "Нет" ? Convert(job, filter) : job)
                ;

            return jobs ?? [];
        }

        private async Task<IEnumerable<Job>> GetJobsAsync(string? url, JobFinderSetting? setting)
        {
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
                var anchor = node.Descendants("a").FirstOrDefault(node =>
                    node.Attributes["href"] != null && node.Attributes["href"].Value.Contains(setting.HrefPrefix) && node.InnerText.Trim().Any());

                var href = ConvertSpecialSymbols(anchor?.Attributes["href"].Value);

                if (anchor != null)
                {
                    var descendants = node.Descendants();

                    var job = new Job
                    {
                        Source = setting?.Source,
                        Link = setting.AddBaseUrlToHrefPrefix ? setting.BaseUrl + href : href,
                        Title = GetTitle(anchor.InnerText),
                        OriginalSalary = GetSalary(descendants, setting),
                        Company = GetInnerText(descendants, setting.Company),
                        Experience = GetInnerText(descendants, setting.Experience),
                        Location = GetInnerText(descendants, setting.Location),
                        TimePosted = GetInnerText(descendants, setting.TimePosted),
                        Logo = new Logo { Source = setting.Source, Url = url }
                    };

                    if (job.OriginalSalary is not null) 
                    {
                        job.Salary = new Salary
                        {
                            Min = job.OriginalSalary.Min,
                            Max = job.OriginalSalary.Max,
                            Currency = job.OriginalSalary.Currency
                        };
                    }

                    yield return job;
                }
            }
        }


        private string? GetTitle(string title)
        {
            return ConvertSpecialSymbols(title);
        }

        private Salary? GetSalary(IEnumerable<HtmlNode> nodes, JobFinderSetting setting)
        {
            var innerText = string.IsNullOrEmpty(setting.Salary?.Value)
                ? nodes.FirstOrDefault(x => ContainsCurrencySymbols(x.InnerText))?.InnerText
                : nodes.FirstOrDefault(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains(setting.Salary.Value))?.InnerText;

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
                int range = 30;

                int start = Math.Max(0, index - (index < range ? index : range));
                int length = Math.Min(innerText.Length - start, (innerText.Length - 1 < range ? innerText.Length : range));

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
                var x when x(tenge) => "₸",
                var x when x(lari) => "₾",
                var x when x(manat) => "₼",
                var x when x(som) => "so'm",
                _ => null
            };

            var salaryMatch = Regex.Match(innerText, @"(\d[\d\s]*)\s*[-–—]\s*(\d[\d\s]*)|(\d[\d\s]*)");

            if (salaryMatch.Success)
            {
                string minStr = salaryMatch.Groups[1].Success ? salaryMatch.Groups[1].Value : salaryMatch.Groups[3].Value;
                string maxStr = salaryMatch.Groups[2].Success ? salaryMatch.Groups[2].Value : minStr;

                minStr = Regex.Replace(minStr, @"\s+", "");
                maxStr = Regex.Replace(maxStr, @"\s+", "");

                salary.Min = ParseSalary(minStr);
                salary.Max = ParseSalary(maxStr);

                return salary;
            }
            else
            {
                var singleMatch = Regex.Match(innerText, @"(\d[\d\s]*)");
                if (singleMatch.Success)
                {
                    var numStr = Regex.Replace(singleMatch.Groups[1].Value, @"\s+", "");
                    salary.Min = salary.Max = ParseSalary(numStr);
                    return salary;
                }
            }

            return null;
        }

        private bool ContainsCurrencySymbols(string innerText)
        {
            return currencies.Any(innerText.Contains);
        }

        private static string? ConvertSpecialSymbols(string? innerText)
        {
            return WebUtility.HtmlDecode(innerText?
                .Replace("<!--", "")?
                .Replace("-->", "")
                .Replace("\n", "")
                .Replace("\t", "")
                .Trim());
        }

        private int ParseSalary(string salary)
        {
            salary = Regex.Replace(salary, @"\s+", " ").Trim();
            salary = Regex.Replace(salary, @"(?<=\d)\s+(?=\d)", "");

            return int.Parse(salary);
        }

        private string? GetInnerText(IEnumerable<HtmlNode> nodes, Models.HtmlAttribute? cssAttribute)
        {
            if (cssAttribute is null)
            {
                return null;
            }
                
            var innerText = nodes.FirstOrDefault(x => (x.Attributes[$"{cssAttribute.Attribute}"]?.Value ?? "")
                .Contains(cssAttribute?.Value ?? ""))?.InnerText;

            return ConvertSpecialSymbols(innerText);
        }

        private Job? Convert(Job? job, JobsFilter? filter)
        {
            if (job?.Salary is null || job.Salary?.Currency == filter?.Salary?.Currency)
            {
                return job;
            }

            var jobCurrencyData = filter?.CurrencyRates?.FirstOrDefault(rate => rate.Abbreviation == currenciesApi[job?.Salary?.Currency]);
            var apiCurrencyData = filter?.CurrencyRates?.FirstOrDefault(rate => rate.Abbreviation == currenciesApi[filter?.Salary?.Currency]);

            var jobRate = jobCurrencyData?.Rate / jobCurrencyData?.Scale;
            var convertRate = apiCurrencyData?.Rate / apiCurrencyData?.Scale;

            var rate = jobRate / convertRate;

            job?.Salary?.Min = (int?)Math.Round((float)(job.Salary!.Min * rate));
            job?.Salary?.Max = (int?)Math.Round((float)(job.Salary!.Max * rate));
            job?.Salary?.Currency = filter?.Salary?.Currency;

            return job;
        }
    }
}
