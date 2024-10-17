﻿using HtmlAgilityPack;
using JobFinder.WebApp.Enums;
using JobFinder.WebApp.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using NickBuhro.Translit;

namespace JobFinder.WebApp.Controllers
{
    [EnableCors("AllowAll")]
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
            var nodes = requestModel?.Source?.ToUpper() switch
            {
                SourceNames.RabotaBy => GetNodesCore($"{rabotaBy}{requestModel.Speciality} {requestModel.Area}", "vacancy-info", requestModel.Source),
                SourceNames.DevBy => GetNodesCore(await GetUrlForDevBy(requestModel), "vacancies-list-item", requestModel.Source),
                SourceNames.PracaBy => GetNodesCore($"{pracaBy}{requestModel.Speciality}+{Transliteration.LatinToCyrillic(requestModel.Area)}", "vac-small__column vac-small__column_2", requestModel.Source),
                SourceNames.LinkedIn => GetNodesCore($"{linkedIn}{requestModel.Speciality} {requestModel.Area}", "", requestModel.Source),
                SourceNames.Trabajo => GetNodesCore($"{trabajo}{requestModel.Speciality!.Trim('.', ',')}/{Transliteration.LatinToCyrillic(requestModel.Area)}", "job-item", requestModel.Source),
                SourceNames.BeBee => GetNodesCore($"{bebee}{requestModel.Speciality}&location={requestModel.Area}", "clickable-job", requestModel.Source),
                SourceNames.JobLum => GetNodesCore($"{joblum}{requestModel.Speciality}&sort=0&lo%5B%5D={Transliteration.LatinToCyrillic(requestModel.Area)}", "result-wrp row", requestModel.Source),
                _ => Task.Run(Enumerable.Empty<ResponseModel>)
            };

            return await nodes?? [];
        }

        private static async Task<IEnumerable<ResponseModel>> GetNodesCore(string url, string className, string source)
        {
            HtmlDocument? doc = null;
            doc = await new HtmlWeb().LoadFromWebAsync(url);

            var nodes = (source.ToUpper() switch
            {
                SourceNames.Trabajo => doc?.DocumentNode?.Descendants("li"),
                SourceNames.BeBee => doc?.DocumentNode?.Descendants("li"),
                _ => doc?.DocumentNode?.Descendants("div")
            })?.Where(n => n?.Attributes["class"] != null ? n.Attributes["class"].Value.Contains($"{className}") : false) ?? [];

            return GetNodesCoreIterator();

            IEnumerable<ResponseModel> GetNodesCoreIterator()
            {
                foreach (var node in nodes)
                {
                    var anchor = node.Descendants("a").FirstOrDefault(a => a.Attributes["href"] != null && IsJobRefference(a, source) && a.InnerText.Trim().Any());
                    var href = anchor?.Attributes["href"].Value;

                    if (anchor != null)
                    {
                        yield return new ResponseModel
                        {
                            Link = source.ToUpper() switch
                            {
                                SourceNames.JobLum => "https://by.joblum.com/" + href,
                                SourceNames.DevBy => "https://jobs.devby.io/" + href,
                                _ => href
                            },
                            Title = source.ToUpper() switch
                            {
                                SourceNames.JobLum => ConvertSpecialSymbols(anchor.Attributes["title"]?.Value),
                                _ => ConvertSpecialSymbols(anchor.InnerText)
                            } + " ",
                            Salary = GetSalaryValue(node, source)
                        };
                    }
                }
            }
        }

        private static bool IsJobRefference(HtmlNode node, string? resource)
        {
            static bool ContainsUri(HtmlNode x, string uri)
            { 
                return x.Attributes["href"].Value.Contains(uri);
            }

            return resource?.ToUpper() switch
            {
                SourceNames.RabotaBy => ContainsUri(node, "rabota.by/vacancy/") || ContainsUri(node, "hh.ru/vacancy/"),
                SourceNames.DevBy => ContainsUri(node, "/vacancies/"),
                SourceNames.PracaBy => ContainsUri(node, "praca.by/vacancy/"),
                SourceNames.LinkedIn => ContainsUri(node, "/job/"),
                SourceNames.Trabajo => ContainsUri(node, "by.trabajo.org/работа-"),
                SourceNames.BeBee => ContainsUri(node, "by.bebee.com/job/"),
                SourceNames.JobLum => ContainsUri(node, "/job/"),
                _ => false
            };
        }

        private static string? GetSalaryValue(HtmlNode node, string? source)
        {
            var spans = node.Descendants("span");

            var result = source?.ToUpper() switch
            {
                SourceNames.RabotaBy => spans.FirstOrDefault(x => x.Attributes["data-sentry-element"] != null)?.InnerText,
                SourceNames.DevBy => node.Descendants("div").FirstOrDefault(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("vacancies-list-item__salary"))?.InnerText,
                SourceNames.PracaBy => spans.FirstOrDefault(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("salary-dotted"))?.InnerText,
                SourceNames.LinkedIn => $"",
                SourceNames.Trabajo => string.Empty,
                SourceNames.BeBee => $"",
                SourceNames.JobLum => ConvertSpecialSymbols(node.Descendants("div").FirstOrDefault(x => x.Attributes["class"] == null)?.InnerText),
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
    }
}
