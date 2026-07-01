using System.Text.RegularExpressions;

using HtmlAgilityPack;

using JobFinders.Server.Enums;
using JobFinders.Server.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using NickBuhro.Translit;

namespace JobFinders.Server.Controllers
{
    [EnableCors("AllowClient")]
    public class JobsController : Controller
    {
        private const string areaPlaceholder = "#area#";
        private const string specialityPlaceholder = "#speciality#";

        private readonly string rabotaByBaseUrl;
        private readonly string devByBaseUrl;
        private readonly string pracaByBaseUrl;
        private readonly string linkedInBaseUrl;
        private readonly string trabajoBaseUrl;
        private readonly string bebeeBaseUrl;
        private readonly string joblumBaseUrl;

        public JobsController()
        {
            rabotaByBaseUrl = $"https://rabota.by/search/vacancy/?text={areaPlaceholder} {specialityPlaceholder}";
            devByBaseUrl = $"https://jobs.devby.io/?[city_id][]={areaPlaceholder}&filter[search]={specialityPlaceholder}";
            pracaByBaseUrl = $"https://praca.by/rabota-{areaPlaceholder}/?search%5Bquery%5D={specialityPlaceholder}";
            linkedInBaseUrl = $"https://www.linkedin.com/search/results/all/?&keywords={areaPlaceholder} {specialityPlaceholder}";
            trabajoBaseUrl = $"https://by.trabajo.org/работы-{specialityPlaceholder}/{areaPlaceholder}";
            bebeeBaseUrl = $"https://bebee.com/by/jobs?q={specialityPlaceholder}&location={areaPlaceholder}";
            joblumBaseUrl = $"https://by.joblum.com/jobs?q={specialityPlaceholder}&sort=0&lo%5B%5D={areaPlaceholder}";
        }

        [HttpPost]
        public async Task<IActionResult> GetList([FromBody] JobsRequestModel requestModel)
        {
            var responseModels = await GetNodeSequence(requestModel);

            return responseModels != null ? Ok(responseModels) : BadRequest();
        }

        private async Task<IEnumerable<JobsResponseModel>> GetNodeSequence(JobsRequestModel requestModel)
        {
            var response = new List<JobsResponseModel>();

            if (requestModel.Sources == null)
            { 
                return response;
            }

            foreach (var source in requestModel.Sources)
            {
                var jobFinder = Enum.Parse<JobFindersEnum>(source, ignoreCase: true);

                var responseModel = new JobsResponseModel();
                responseModel.SourceName = source;
                var sourceUrl = "";

                responseModel.Jobs = jobFinder switch
                {
                    JobFindersEnum.RabotaBy => await GetJobsCore(GetSourceUrl(rabotaByBaseUrl, requestModel.Speciality, requestModel.Area, TransliterationEnum.Cyrillic, ref sourceUrl), "vacancy-info", jobFinder),
                    JobFindersEnum.DevBy => await GetJobsCore(GetSourceUrl(devByBaseUrl, requestModel.Speciality, requestModel.Area, TransliterationEnum.Latin, ref sourceUrl), "vacancies-list-item", jobFinder),
                    JobFindersEnum.PracaBy => await GetJobsCore(GetSourceUrl(pracaByBaseUrl, requestModel.Speciality, requestModel.Area, TransliterationEnum.Latin, ref sourceUrl), "vac-small__column vac-small__column_2", jobFinder),
                    //JobFindersEnum.LinkedIn => await GetJobsCore($"{linkedIn}{requestModel.Speciality} {requestModel.Area}", "", jobFinder),
                    JobFindersEnum.Trabajo => await GetJobsCore(GetSourceUrl(trabajoBaseUrl, requestModel.Speciality, requestModel.Area, TransliterationEnum.Latin, ref sourceUrl), "job-item", jobFinder),
                    JobFindersEnum.BeBee => await GetJobsCore(GetSourceUrl(bebeeBaseUrl, requestModel.Speciality, requestModel.Area, TransliterationEnum.Latin, ref sourceUrl), "group relative", jobFinder),
                    JobFindersEnum.Joblum => await GetJobsCore(GetSourceUrl(joblumBaseUrl, requestModel.Speciality, requestModel.Area, TransliterationEnum.Cyrillic, ref sourceUrl), "result-wrp row", jobFinder),
                    _ => await Task.Run(Enumerable.Empty<JobModel>)
                };

                responseModel.SourceUrl = sourceUrl;

                response.Add(responseModel);
            }

            return response;
        }

        private static async Task<IEnumerable<JobModel>> GetJobsCore(string url, string? className, JobFindersEnum jobFinder)
        {
            HtmlDocument? doc = null;
            doc = await new HtmlWeb().LoadFromWebAsync(url);

            var nodes = (jobFinder switch
            {
                JobFindersEnum.Trabajo => doc?.DocumentNode?.Descendants("li"),
                //JobFindersEnum.BeBee => doc?.DocumentNode?.Descendants("li"),
                JobFindersEnum.BeBee => doc?.DocumentNode?.Descendants("article"),
                _ => doc?.DocumentNode?.Descendants("div")
            })?.Where(n => n?.Attributes["class"] != null ? n.Attributes["class"].Value.Contains($"{className}") : false) ?? [];

            var jobs = Enumerable.Empty<JobModel>();

            try
            {
                jobs = GetJobsCoreIterator().DistinctBy(x => x.Link);
            }
            catch (Exception ex)
            {
                jobs = Enumerable.Empty<JobModel>().Append(new JobModel { Title = "Server Error" });
            }

            return jobs;

            IEnumerable<JobModel> GetJobsCoreIterator()
            {
                foreach (var node in nodes)
                {
                    var anchor = node.Descendants("a").FirstOrDefault(a => a.Attributes["href"] != null && IsJobRefference(a, jobFinder) && a.InnerText.Trim().Any());
                    var href = anchor?.Attributes["href"].Value;

                    if (anchor != null)
                    {
                        yield return new JobModel
                        {
                            Link = jobFinder switch
                            {
                                JobFindersEnum.Joblum => "https://by.joblum.com/" + href,
                                JobFindersEnum.DevBy => "https://jobs.devby.io" + href,
                                JobFindersEnum.BeBee => "https://bebee.com/" + href,
                                JobFindersEnum.PracaBy => "https://praca.by" + href,
                                _ => href
                            },
                            Title = jobFinder switch
                            {
                                JobFindersEnum.Joblum => ConvertSpecialSymbols(anchor.Attributes["title"]?.Value),
                                _ => ConvertSpecialSymbols(anchor.InnerText)
                            } + " ",
                            Salary = GetSalaryValue(node, jobFinder)
                        };
                    }
                }
            }
        }

        private static bool IsJobRefference(HtmlNode node, JobFindersEnum JobFinder)
        {
            static bool ContainsUri(HtmlNode x, string uri)
            {
                return x.Attributes["href"].Value.Contains(uri);
            }

            return JobFinder switch
            {
                JobFindersEnum.RabotaBy => ContainsUri(node, "rabota.by/vacancy/") || ContainsUri(node, "hh.ru/vacancy/"),
                JobFindersEnum.DevBy => ContainsUri(node, "/vacancies/"),
                JobFindersEnum.PracaBy => ContainsUri(node, "/vacancy/"),
                JobFindersEnum.LinkedIn => ContainsUri(node, "/job/"),
                JobFindersEnum.Trabajo => ContainsUri(node, "by.trabajo.org/работа-"),
                JobFindersEnum.BeBee => ContainsUri(node, "/by/jobs/"),
                JobFindersEnum.Joblum => ContainsUri(node, "/job/"),
                _ => false
            };
        }

        private static string? GetSalaryValue(HtmlNode node, JobFindersEnum jobFinder)
        {
            string[] currencies = { "$", "€", "Br", "руб", "AED", "USD", "EUR", "SEK", "NOK" };

            bool ContainsCurrencySymbols(string innerText)
            {
                //var pattern = @"(?i)(\$|€|\bBr\b|\bруб\b|\bAED\b|\bUSD\b|\bEUR\b|\bSEK\b|\bNOK\b)";
                //return innerText.Length < 20 && Regex.IsMatch(innerText, pattern);

                if (innerText.Length > 100)
                {
                    return false;
                }

                foreach (var c in currencies)
                {
                    // Symbols: match directly
                    if (c == "$" || c == "€")
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

            var descendants = node.Descendants();

            var result = jobFinder switch
            {
                JobFindersEnum.RabotaBy => descendants.FirstOrDefault(x => ContainsCurrencySymbols(x.InnerText))?.InnerText,
                JobFindersEnum.DevBy => node.Descendants("div").FirstOrDefault(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("vacancies-list-item__salary"))?.InnerText,
                JobFindersEnum.PracaBy => node.Descendants("span").FirstOrDefault(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("salary-dotted"))?.InnerText,
                //JobFindersEnum.LinkedIn => string.Empty,
                //JobFindersEnum.Trabajo => string.Empty,
                JobFindersEnum.BeBee => descendants.FirstOrDefault(x => ContainsCurrencySymbols(x.InnerText))?.InnerText,
                JobFindersEnum.Joblum => ConvertSpecialSymbols(node.Descendants("div").FirstOrDefault(x => x.Attributes["class"] == null)?.InnerText),
                _ => null
            };

            return ConvertSpecialSymbols(result);
        }

        private static string? ConvertSpecialSymbols(string? line)
        {
            var cleaned = line?.Replace("&nbsp;", " ")?
                .Replace("&quot;", "\"")?
                .Replace("&amp;", "&")?
                .Replace("&mdash;", "-")?
                .Replace("<!--", "")?
                .Replace("-->", "");

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

        private static string? GetSourceUrl(string? baseUrl, string? speciality, string? area, TransliterationEnum transliteration, ref string? sourceUrl)
        {
            area = transliteration switch
            {
                TransliterationEnum.Cyrillic => Transliteration.LatinToCyrillic(area),
                _ => Transliteration.CyrillicToLatin(area)
            };

            sourceUrl = baseUrl?.Replace(areaPlaceholder, area).Replace(specialityPlaceholder, speciality);
           return sourceUrl;
        }
    }
}