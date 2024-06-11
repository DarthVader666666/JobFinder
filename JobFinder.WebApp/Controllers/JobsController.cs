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
            var responseModels = requestModel.Source switch
            {
                var source when GetParseResult(source, SourceNames.RabotaBy) => GetRabotaByResponseModels(requestModel),
                var source when GetParseResult(source, SourceNames.DevBy) => GetDevByResponseModels(requestModel),
                var source when GetParseResult(source, SourceNames.PracaBy) => GetPracaByResponseModels(requestModel),
                //var source when GetParseResult(source, SourceNames.LinkedIn) => GetLinkedInResponseModels(requestModel),
                _ => null
            };

            return responseModels != null ? Ok(responseModels) : BadRequest();
        }

        private static async IAsyncEnumerable<ResponseModel>? GetRabotaByResponseModels(RequestModel requestModel)
        {
            int page = 1;

            while (page < 5)
            {
                var url = requestModel.Url + $"text={requestModel.Speciality}" +
                    (requestModel.Area?.Length > 0 ? $" {requestModel.Area}" : "") + $"&page={page - 1}";

                var doc = await new HtmlWeb().LoadFromWebAsync(url);
                var nodes = doc.DocumentNode.SelectNodes("//div/h2/span/a[@href]");
                var nodeSequence = nodes?.Where(x => x.Attributes["href"].Value.Contains("rabota.by/vacancy/")
                    || x.Attributes["href"].Value.Contains("hh.ru/vacancy/"));

                if (nodeSequence != null && nodeSequence.Any())
                {
                    foreach (HtmlNode node in nodeSequence)
                    {
                        HtmlAttribute href = node.Attributes["href"];
                        yield return new ResponseModel { Link = href.Value, Title = node.InnerText };
                    }

                    page++;
                }
                else
                {
                    break;
                }
            }
        }

        private static async IAsyncEnumerable<ResponseModel>? GetDevByResponseModels(RequestModel requestModel)
        {
            requestModel.Area = Transliteration.LatinToCyrillic(requestModel.Area);

            var url = requestModel.Url;
            var doc = await new HtmlWeb().LoadFromWebAsync(url);

            var cityCode = doc.DocumentNode?.SelectNodes("//select/option")?
                .FirstOrDefault(x => x.InnerText.Equals(requestModel?.Area, StringComparison.InvariantCultureIgnoreCase))?
                .Attributes["value"].Value;

            url = requestModel.Url + $"filter[search]={requestModel.Speciality}" + 
                (requestModel.Area?.Length > 0 ? $"&filter[city_id][]={cityCode}" : "");

            doc = await new HtmlWeb().LoadFromWebAsync(url);
            var nodes = doc.DocumentNode.SelectNodes("//div/a[@href]");

            var nodeSequence = nodes?.Where(x => x.Attributes["href"].Value.Contains("/vacancies/"));

            if (nodeSequence != null && nodeSequence.Any())
            {
                foreach (HtmlNode node in nodeSequence)
                {
                    HtmlAttribute href = node.Attributes["href"];
                    yield return new ResponseModel { Link = requestModel.Url?[..(requestModel.Url.Length - 2)] + href.Value, Title = node.InnerText };
                }
            }
        }

        private static async IAsyncEnumerable<ResponseModel>? GetPracaByResponseModels(RequestModel requestModel)
        {
            requestModel.Area = Transliteration.LatinToCyrillic(requestModel.Area);

            var url = requestModel.Url + $"search%5Bquery%5D={requestModel.Speciality}" + (requestModel.Area?.Length > 0 ? $"+{requestModel.Area}" : "");

            var doc = await new HtmlWeb().LoadFromWebAsync(url);
            var nodes = doc.DocumentNode.SelectNodes("//div/a[@href]");

            var nodeSequence = nodes?.Where(x => x.Attributes["href"].Value.Contains("/vacancy/") && x.InnerText.Length > 0);

            if (nodeSequence != null && nodeSequence.Any())
            {
                foreach (HtmlNode node in nodeSequence)
                {
                    HtmlAttribute href = node.Attributes["href"];
                    yield return new ResponseModel { Link = href.Value, Title = node.InnerText };
                }
            }
        }

        private static async IAsyncEnumerable<ResponseModel> GetLinkedInResponseModels(RequestModel requestModel)
        {
            var url = requestModel?.Url + $"&keywords={requestModel?.Speciality}"
                    + (requestModel?.Area?.Length > 0 ? $" {requestModel.Area}" : "");

            var doc = await new HtmlWeb().LoadFromWebAsync(url);

            var urls = doc.DocumentNode.SelectNodes("//a");
                
            url = urls.FirstOrDefault(x => 
                x.InnerText.Contains("See all job results"))?.Attributes["href"].Value;

            doc = await new HtmlWeb().LoadFromWebAsync(url);

            var nodes = doc.DocumentNode.SelectNodes("//div/a[@href]");

            var nodeSequence = nodes?.Where(x => x.Attributes["href"].Value.Contains("jobs/view")
                && SpecialityMatches(requestModel?.Speciality ?? "", x.InnerText));

            if (nodeSequence != null && nodeSequence.Any())
            {
                foreach (HtmlNode node in nodeSequence)
                {
                    HtmlAttribute href = node.Attributes["href"];
                    var responseModel = new ResponseModel { Link = href.Value, Title = node.InnerText.Trim() };

                    yield return responseModel;
                }
            }
        }




        private static bool GetParseResult(string? source, string sourceName)
        {
            return source?.ToUpper() == sourceName;
        }

        private static bool SpecialityMatches(string speciality, string innerText)
        { 
            var lines = speciality.Split(' ');

            foreach (var item in lines)
            {
                if (innerText.Contains(item, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
