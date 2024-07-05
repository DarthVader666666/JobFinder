using HtmlAgilityPack;
using JobFinder.WebApp.Enums;
using JobFinder.WebApp.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NickBuhro.Translit;

namespace JobFinder.WebApp.Controllers
{
    [EnableCors("AllowAll")]
    public class JobsController : Controller
    {
        [HttpPost]
        public IActionResult GetList([FromBody] RequestModel requestModel)
        {
            var responseModels = GetResponseModels(requestModel);

            return responseModels != null ? Ok(responseModels) : BadRequest();
        }

        private static IEnumerable<ResponseModel>? GetResponseModels(RequestModel requestModel)
        {
            var nodeSequence = GetNodeSequence(requestModel).Result;

            if (nodeSequence != null && nodeSequence.Any())
            {
                foreach (HtmlNode node in nodeSequence)
                {
                    HtmlAttribute href = node.Attributes["href"];

                    yield return new ResponseModel 
                    { 
                        Link = requestModel?.Source?.ToUpper() switch
                        {
                            SourceNames.JobLum => "https://by.joblum.com/" + href.Value,
                            SourceNames.DevBy => "https://jobs.devby.io/" + href.Value,
                            _ => href.Value
                        },
                        Title = node.InnerText 
                    };
                }
            }
        }

        private static async Task<IEnumerable<HtmlNode>?> GetNodeSequence(RequestModel requestModel)
        {
            HtmlDocument? doc = null;
            string? url = null;

            if (requestModel?.Source?.ToUpper() == SourceNames.DevBy)
            {
                doc = await new HtmlWeb().LoadFromWebAsync("https://jobs.devby.io");

                requestModel.Area = doc.DocumentNode?.SelectNodes("//select/option")?
                    .FirstOrDefault(x => x.InnerText.Equals(Transliteration.LatinToCyrillic(requestModel.Area), StringComparison.InvariantCultureIgnoreCase))?
                    .Attributes["value"].Value;
            }

            url = GetUrl(requestModel?.Source?.ToUpper(), requestModel?.Speciality, requestModel?.Area);
            doc = await new HtmlWeb().LoadFromWebAsync(url);

            var nodes = doc?.DocumentNode?.SelectNodes("//a[@href]");

            var nodeSequence = nodes?.Where(x => IsJobRefference(x, requestModel?.Source?.ToUpper()) && x.InnerText.Trim().Any());

            return nodeSequence;
        }

        private static bool IsJobRefference(HtmlNode node, string? resource)
        {
            static bool ContainsUri(HtmlNode x, string uri)
            { 
                return x.Attributes["href"].Value.Contains(uri);
            }

            return resource switch
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

        private static string? GetUrl(string? resource, string? speciality, string? area, int page = 1)
        {
            return resource switch
            {
                SourceNames.RabotaBy => $"https://rabota.by/search/vacancy/?text={speciality} {area}&page={page - 1}",
                SourceNames.DevBy => $"https://jobs.devby.io/?filter[search]={speciality}&filter[city_id][]={area}",
                SourceNames.PracaBy => $"https://praca.by/search/vacancies/?search%5Bquery%5D={speciality}+{Transliteration.LatinToCyrillic(area)}",
                SourceNames.LinkedIn => $"https://www.linkedin.com/search/results/all/?&keywords={speciality} {area}",
                SourceNames.Trabajo => $"https://by.trabajo.org/работы-{speciality!.Trim('.',',')}/{Transliteration.LatinToCyrillic(area)}",
                SourceNames.BeBee => $"https://by.bebee.com/jobs?term={speciality}&location={area}",
                SourceNames.JobLum => $"https://by.joblum.com/jobs?q={speciality}&sort=0&lo%5B%5D={Transliteration.LatinToCyrillic(area)}",
                _ => null
            };
        }
    }
}
