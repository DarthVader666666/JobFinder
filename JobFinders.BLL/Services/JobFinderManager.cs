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

        public async Task<(IEnumerable<Job>? jobs, string? link)> ProcessAsync(string? speciality, string? location, JobFinderSetting? setting)
        {
            var transliteration = Enum.Parse<TransliterationEnum>(setting.LocationTransliteration);

            location ??= setting.MandatoryLocation ? "minsk" : string.Empty;

            location = transliteration switch
            {
                TransliterationEnum.Latin => Transliteration.CyrillicToLatin(location),
                TransliterationEnum.Cyrillic => Transliteration.LatinToCyrillic(location),
            };

            speciality ??= string.Empty;

            var link = setting.LinkTemplate?.Replace(locationPlaceholder, location).Replace(specialityPlaceholder, speciality);

            if (setting == null)
            {
                throw new Exception("JobFinderSetting not found");
            }

            var jobs = await GetJobsAsync(speciality, location, link, setting);

            return (jobs, link);
        }

        private async Task<IEnumerable<Job>> GetJobsAsync(string? speciality, string? location, string? url, JobFinderSetting? setting)
        {
            HtmlDocument? doc = null;

            try
            {
                doc = await new HtmlWeb().LoadFromWebAsync(url);
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<Job>().Append(new Job { Title = "Server Error" });
            }

            var nodes = doc?.DocumentNode?.Descendants(setting.NodeTag)
                .Where(n => n?.Attributes["class"] != null && n.Attributes["class"].Value.Contains($"{setting.TagCssClass}")) ?? [];

            var jobs = Enumerable.Empty<Job>();

            try
            {
                jobs = JobsIterator(setting, nodes).DistinctBy(x => x.Link);
            }
            catch (Exception ex)
            {
                jobs = Enumerable.Empty<Job>().Append(new Job { Title = "Server Error" });
            }

            return jobs;            
        }

        private IEnumerable<Job> JobsIterator(JobFinderSetting setting, IEnumerable<HtmlNode> nodes)
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
                        Title = ConvertSpecialSymbols(anchor.InnerText),
                        Salary = ConvertSpecialSymbols(string.IsNullOrEmpty(setting.SalaryCssClass)
                            ? descendants.FirstOrDefault(x => ContainsCurrencySymbols(x.InnerText))?.InnerText
                            : descendants.FirstOrDefault(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains(setting.SalaryCssClass))?.InnerText),
                    };
                }
            }
        }

        private bool ContainsCurrencySymbols(string innerText)
        {
            string[] currencies = { "$", "€", "Br", "BYN", "руб", "AED", "USD", "EUR", "SEK", "NOK", "₽" };

            var pattern = @"(?i)(\$|€|\bBr\b|\bруб\b|\bAED\b|\bUSD\b|\bEUR\b|\bSEK\b|\bNOK\b)";
            //return innerText.Length < 20 && Regex.IsMatch(innerText, pattern);

            if (innerText.Length > 100)
            {
                return false;
            }

            foreach (var c in currencies)
            {
                // Symbols: match directly
                if (c == "$" || c == "€" || c == "₽")
                {
                    if (innerText.Contains(c))
                        return true;
                }
                else
                {
                    // Alphabetic currencies: match whole words
                    if (Regex.IsMatch(innerText, $@"(?i)\b{Regex.Escape(c)}\b"))
                        return true;
                }
            }
            return false;
        }

        //private static string? GetSalaryValue(HtmlNode node, JobSourcesEnum jobFinder)
        //{
        //    var descendants = node.Descendants();

        //    var result = jobFinder switch
        //    {
        //        JobSourcesEnum.RabotaBy => descendants.FirstOrDefault(x => ContainsCurrencySymbols(x.InnerText))?.InnerText,
        //        JobSourcesEnum.DevBy => node.Descendants("div").FirstOrDefault(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("vacancies-list-item__salary"))?.InnerText,
        //        JobSourcesEnum.PracaBy => node.Descendants("span").FirstOrDefault(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("salary-dotted"))?.InnerText,
        //        //JobFindersEnum.LinkedIn => string.Empty,
        //        //JobFindersEnum.Trabajo => string.Empty,
        //        JobSourcesEnum.BeBee => descendants.FirstOrDefault(x => ContainsCurrencySymbols(x.InnerText))?.InnerText,
        //        JobSourcesEnum.Joblum => ConvertSpecialSymbols(node.Descendants("div").FirstOrDefault(x => x.Attributes["class"] == null)?.InnerText),
        //        _ => null
        //    };

        //    return ConvertSpecialSymbols(result);
        //}

        private static string? ConvertSpecialSymbols(string? line)
        {
            var cleaned = line?.Replace("&nbsp;", " ")?
                .Replace("&quot;", "\"")?
                .Replace("&amp;", "&")?
                .Replace("&mdash;", "-")?
                .Replace("<!--", "")?
                .Replace("-->", "")
                .Trim();

            var match = Regex.Match(cleaned ?? "",
                @"(?i)(\d[\d \s,.]*)(?:\s*[-–—]\s*(\d[\d \s,.]*))?\s*(Br|\$|AED|руб|€)");

            if (match.Success)
            {
                var first = match.Groups[1].Value.Trim();
                var second = match.Groups[2].Success ? match.Groups[2].Value.Trim() : null;
                var currency = match.Groups[3].Value.Trim();

                return second != null
                    ? $"{first} - {second} {currency}"
                    : $"{first} {currency}";
            }

            return cleaned;
        }


        //private static async Task<string> GetUrlForDevBy(RequestModel requestModel)
        //{
        //    var doc = await new HtmlWeb().LoadFromWebAsync("https://jobs.devby.io");

        //    var area = doc.DocumentNode?.SelectNodes("//select/option")?
        //        .FirstOrDefault(x => x.InnerText.Equals(Transliteration.LatinToCyrillic(requestModel.Area), StringComparison.InvariantCultureIgnoreCase))?
        //        .Attributes["value"].Value;

        //    return $"{devBy}{requestModel.Speciality}&filter[city_id][]={area}";
        //}
    }
}
