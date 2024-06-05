using HtmlAgilityPack;
using JobFinder.WebApp.Enums;
using JobFinder.WebApp.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

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
                var source when GetParseResult(source, SourceNames.LinkedIn) => GetLinkedInResponseModels(requestModel),
                _ => null
            };

            return responseModels != null ? Ok(responseModels) : BadRequest();
        }

        private static async IAsyncEnumerable<ResponseModel> GetRabotaByResponseModels(RequestModel requestModel)
        {
            int page = 1;

            while (page < 5)
            {
                var url = requestModel.Url + $"text={requestModel.Speciality}" +
                    (requestModel.Area?.Length > 0 ? $" {requestModel.Area}" : "") + $"&page={page - 1}";

                var doc = await new HtmlWeb().LoadFromWebAsync(url);
                var pageNode = doc.DocumentNode.SelectNodes("//span").FirstOrDefault(x => x.InnerText == $"{page}");

                if (pageNode != null)
                {
                    var nodes = doc.DocumentNode.SelectNodes("//div/h2/span/a[@href]");
                    var nodeSequence = nodes?.Where(x => x.Attributes["href"].Value.Contains("rabota.by/vacancy/") || 
                            x.Attributes["href"].Value.Contains("hh.ru/vacancy/"));

                    if (nodeSequence != null)
                    {
                        foreach (HtmlNode node in nodeSequence)
                        {
                            HtmlAttribute href = node.Attributes["href"];
                            yield return new ResponseModel { Link = href.Value, Title = node.InnerText };
                        }
                    }
                    else
                    {
                        break;
                    }

                    page++;
                }
                else
                {
                    break;
                }
            }
        }

        private static async IAsyncEnumerable<ResponseModel> GetLinkedInResponseModels(RequestModel requestModel)
        {
            var url = requestModel.Url + $"keywords={requestModel.Speciality}" +
                (requestModel.Area?.Length > 0 ? $"&location={requestModel.Area}" : "");

            var doc = await new HtmlWeb().LoadFromWebAsync(url);

            if (doc != null)
            {
                var nodes = doc.DocumentNode.SelectNodes("//div/a[@href]");
                var nodeSequence = nodes?.Where(x => x.Attributes["href"].Value.Contains("jobs/view"));

                if (nodeSequence != null)
                {
                    foreach (HtmlNode node in nodeSequence)
                    {
                        HtmlAttribute href = node.Attributes["href"];
                        var responseModel = new ResponseModel { Link = href.Value, Title = node.InnerText.Trim() };

                        yield return responseModel;
                    }
                }
            }
        }

        private static bool GetParseResult(string? source, string sourceName)
        {
            return source?.ToUpper() == sourceName;
        }
    }
}
