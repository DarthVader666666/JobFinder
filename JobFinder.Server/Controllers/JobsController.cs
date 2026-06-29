﻿using HtmlAgilityPack;
using JobFinder.Server.Enums;
using JobFinder.Server.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using NickBuhro.Translit;

namespace JobFinder.Server.Controllers
{
    [EnableCors("AllowClient")]
    public class JobsController : Controller
    {
        private const string rabotaBy = "https://rabota.by/search/vacancy/?text=";
        private const string devBy = "https://jobs.devby.io/?filter[search]=";
        private const string pracaBy = "https://praca.by/search/vacancies/?search%5Bquery%5D=";
        private const string linkedIn = "https://www.linkedin.com/search/results/all/?&keywords=";
        private const string trabajo = "https://by.trabajo.org/работы-";
        private const string bebee = "https://by.bebee.com/jobs?term=";
        private const string joblum = "https://by.joblum.com/jobs?q=";

        [HttpPost]
        public async Task<IActionResult> GetList([FromBody] RequestModel requestModel)
        {
            var responseModels = await GetNodeSequence(requestModel);

            return responseModels != null ? Ok(responseModels) : BadRequest();
        }

        private static async Task<IEnumerable<ResponseModel>> GetNodeSequence(RequestModel requestModel)
        {
            var response = new List<ResponseModel>();

            if (requestModel.Sources == null)
            { 
                return response;
            }

            foreach (var source in requestModel.Sources)
            {
                var jobFinder = Enum.Parse<JobFinders>(source);

                var responseModel = new ResponseModel();
                responseModel.SourceName = source;
                responseModel.SourceUrl = GetSourceUrl(jobFinder);
                responseModel.Jobs = jobFinder switch
                {
                    JobFinders.RabotaBy => await GetJobsCore($"{rabotaBy}{requestModel.Speciality} {requestModel.Area}", "vacancy-info", jobFinder),
                    JobFinders.DevBy => await GetJobsCore(await GetUrlForDevBy(requestModel), "vacancies-list-item", jobFinder),
                    JobFinders.PracaBy => await GetJobsCore($"{pracaBy}{requestModel.Speciality}+{Transliteration.LatinToCyrillic(requestModel.Area)}", "vac-small__column vac-small__column_2", jobFinder),
                    JobFinders.LinkedIn => await GetJobsCore($"{linkedIn}{requestModel.Speciality} {requestModel.Area}", "", jobFinder),
                    JobFinders.Trabajo => await GetJobsCore($"{trabajo}{requestModel.Speciality!.Trim('.', ',')}/{Transliteration.LatinToCyrillic(requestModel.Area)}", "job-item", jobFinder),
                    JobFinders.BeBee => await GetJobsCore($"{bebee}{requestModel.Speciality}&location={requestModel.Area}", "clickable-job", jobFinder),
                    JobFinders.JobLum => await GetJobsCore($"{joblum}{requestModel.Speciality}&sort=0&lo%5B%5D={Transliteration.LatinToCyrillic(requestModel.Area)}", "result-wrp row", jobFinder),
                    _ => await Task.Run(Enumerable.Empty<JobModel>)
                };

                response.Add(responseModel);
            }

            return response;
        }

        private static async Task<IEnumerable<JobModel>> GetJobsCore(string url, string className, JobFinders jobFinder)
        {
            HtmlDocument? doc = null;
            doc = await new HtmlWeb().LoadFromWebAsync(url);

            var nodes = (jobFinder switch
            {
                JobFinders.Trabajo => doc?.DocumentNode?.Descendants("li"),
                JobFinders.BeBee => doc?.DocumentNode?.Descendants("li"),
                _ => doc?.DocumentNode?.Descendants("div")
            })?.Where(n => n?.Attributes["class"] != null ? n.Attributes["class"].Value.Contains($"{className}") : false) ?? [];

            var jobs = Enumerable.Empty<JobModel>();

            try
            {
                jobs = GetJobsCoreIterator();
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
                                JobFinders.JobLum => "https://by.joblum.com/" + href,
                                JobFinders.DevBy => "https://jobs.devby.io/" + href,
                                _ => href
                            },
                            Title = jobFinder switch
                            {
                                JobFinders.JobLum => ConvertSpecialSymbols(anchor.Attributes["title"]?.Value),
                                _ => ConvertSpecialSymbols(anchor.InnerText)
                            } + " ",
                            Salary = GetSalaryValue(node, jobFinder)
                        };
                    }
                }
            }
        }

        private static bool IsJobRefference(HtmlNode node, JobFinders JobFinder)
        {
            static bool ContainsUri(HtmlNode x, string uri)
            {
                return x.Attributes["href"].Value.Contains(uri);
            }

            return JobFinder switch
            {
                JobFinders.RabotaBy => ContainsUri(node, "rabota.by/vacancy/") || ContainsUri(node, "hh.ru/vacancy/"),
                JobFinders.DevBy => ContainsUri(node, "/vacancies/"),
                JobFinders.PracaBy => ContainsUri(node, "praca.by/vacancy/"),
                JobFinders.LinkedIn => ContainsUri(node, "/job/"),
                JobFinders.Trabajo => ContainsUri(node, "by.trabajo.org/работа-"),
                JobFinders.BeBee => ContainsUri(node, "by.bebee.com/job/"),
                JobFinders.JobLum => ContainsUri(node, "/job/"),
                _ => false
            };
        }

        private static string? GetSalaryValue(HtmlNode node, JobFinders jobFinder)
        {
            var spans = node.Descendants("span");

            var result = jobFinder switch
            {
                JobFinders.RabotaBy => spans.FirstOrDefault(x => x.Attributes["data-sentry-element"] != null)?.InnerText,
                JobFinders.DevBy => node.Descendants("div").FirstOrDefault(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("vacancies-list-item__salary"))?.InnerText,
                JobFinders.PracaBy => spans.FirstOrDefault(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("salary-dotted"))?.InnerText,
                JobFinders.LinkedIn => $"",
                JobFinders.Trabajo => string.Empty,
                JobFinders.BeBee => $"",
                JobFinders.JobLum => ConvertSpecialSymbols(node.Descendants("div").FirstOrDefault(x => x.Attributes["class"] == null)?.InnerText),
                _ => null
            };

            return ConvertSpecialSymbols(result);
        }

        private static string? ConvertSpecialSymbols(string? line)
        {
            return line?.Replace("&nbsp;", "")?.Replace("&quot;", "\"")?.Replace("&mdash;", "-");
        }

        private static async Task<string> GetUrlForDevBy(RequestModel requestModel)
        {
            var doc = await new HtmlWeb().LoadFromWebAsync("https://jobs.devby.io");

            var area = doc.DocumentNode?.SelectNodes("//select/option")?
                .FirstOrDefault(x => x.InnerText.Equals(Transliteration.LatinToCyrillic(requestModel.Area), StringComparison.InvariantCultureIgnoreCase))?
                .Attributes["value"].Value;

            return $"{devBy}{requestModel.Speciality}&filter[city_id][]={area}";
        }

        private static string? GetSourceUrl(JobFinders jobFinder)
        {
            return jobFinder switch
            {
                JobFinders.RabotaBy => rabotaBy,
                JobFinders.DevBy => devBy,
                JobFinders.PracaBy => pracaBy,
                JobFinders.LinkedIn => linkedIn,
                JobFinders.Trabajo => trabajo,
                JobFinders.BeBee => bebee,
                JobFinders.JobLum => joblum,
                _ => null
            };
        }
    }
}