using System.Net;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using JobFinders.Domain.Models;

namespace JobFinders.Application.Services
{
    public partial class HtmlLoader
    {
        private readonly string[] usd = { "$", "USD" };
        private readonly string[] euro = { "€", "EUR" };
        private readonly string[] belRub = { "Br", "BYN", "руб", "" };
        private readonly string[] rusRub = { "₽" };
        private readonly string[] tenge = { "₸" };
        private readonly string[] lari = { "₾" };
        private readonly string[] manat = { "₼" };
        private readonly string[] som = { "so'm" };

        private readonly string[] currencies;

        private Job? Parse(JobFinderSetting? setting, HtmlNode? node, string? url)
        {
            var anchor = node?.Descendants("a").FirstOrDefault(node =>
                    node.Attributes["href"] != null && node.Attributes["href"].Value.Contains(setting?.HrefPrefix ?? "") && node.InnerText.Trim().Any());

            var href = ConvertSpecialSymbols(anchor?.Attributes["href"].Value);

            if (anchor == null)
            {
                return null;
            }

            var descendants = node?.Descendants();

            var job = new Job
            {
                Source = setting?.Source,
                Link = (setting?.AddBaseUrlToHrefPrefix ?? false) ? setting.BaseUrl + href : href,
                Title = GetTitle(anchor.InnerText),
                OriginalSalary = GetSalary(descendants ?? [], setting),
                Company = GetInnerText(descendants, setting?.Company),
                Experience = GetInnerText(descendants, setting?.Experience),
                Location = GetInnerText(descendants, setting?.Location),
                TimePosted = GetInnerText(descendants, setting?.TimePosted),
                Logo = new Logo { Source = setting?.Source, Url = url }
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

            return job;
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

        private string? GetTitle(string title)
        {
            return ConvertSpecialSymbols(title);
        }

        private bool ContainsCurrencySymbols(string innerText)
        {
            return currencies.Any(innerText.Contains);
        }

        private int ParseSalary(string salary)
        {
            salary = Regex.Replace(salary, @"\s+", " ").Trim();
            salary = Regex.Replace(salary, @"(?<=\d)\s+(?=\d)", "");

            return int.Parse(salary);
        }

        private string? GetInnerText(IEnumerable<HtmlNode>? nodes, JobTag? cssAttribute)
        {
            if (cssAttribute is null)
            {
                return null;
            }

            var innerText = nodes?.FirstOrDefault(x => (x.Attributes[$"{cssAttribute.Attribute}"]?.Value ?? "")
                .Contains(cssAttribute?.Value ?? ""))?.InnerText;

            return ConvertSpecialSymbols(innerText);
        }

        private Salary? GetSalary(IEnumerable<HtmlNode>? nodes, JobFinderSetting? setting)
        {
            var innerText = string.IsNullOrEmpty(setting?.Salary?.Value)
                ? nodes?.FirstOrDefault(x => ContainsCurrencySymbols(x.InnerText))?.InnerText
                : nodes?.FirstOrDefault(x => x.Attributes[$"{setting.Salary.Attribute}"] != null && x.Attributes[$"{setting.Salary.Attribute}"].Value.Contains(setting.Salary.Value))?.InnerText;

            if (string.IsNullOrEmpty(innerText))
            {
                return null;
            }

            var salary = new Salary();

            innerText = ConvertSpecialSymbols(innerText);

            var currencyPattern = $@"(?i){string.Join("|", currencies.Select(Regex.Escape))}";
            var currencyMatch = Regex.Match(innerText ?? "", currencyPattern);

            if (currencyMatch.Success)
            {
                int index = currencyMatch.Index;
                int range = 30;

                int start = Math.Max(0, index - (index < range ? index : range));
                int length = Math.Min(innerText?.Length ?? 0 - start, (innerText?.Length - 1 < range ? innerText?.Length ?? 0 : range));

                var substring = innerText?.Substring(start, length);

                if (substring.IsWhiteSpace())
                {
                    start = index;
                    innerText = innerText?.Substring(start, length);
                }
                else if (Regex.IsMatch(substring ?? "", currencyPattern))
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

            innerText = innerText?.Replace(currencyMatch.Value, "");
            var salaryMatch = Regex.Match(innerText ?? "", @"(\d[\d\s]*)\s*(?:[-–—]|\bдо\b)\s*(\d[\d\s]*)|(\d[\d\s]*)");

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
                var singleMatch = Regex.Match(innerText ?? "", @"(\d[\d\s]*)");
                if (singleMatch.Success)
                {
                    var numStr = Regex.Replace(singleMatch.Groups[1].Value, @"\s+", "");
                    salary.Min = salary.Max = ParseSalary(numStr);
                    return salary;
                }
            }

            return null;
        }
    }
}
