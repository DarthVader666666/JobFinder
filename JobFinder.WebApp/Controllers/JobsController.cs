using HtmlAgilityPack;
using JobFinder.WebApp.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace JobFinder.WebApp.Controllers
{
    [EnableCors("AllowAll")]
    public class JobsController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> GetList([FromBody] RequestModel requestModel)
        {
            using var client = new HttpClient();

            var doc = await new HtmlWeb().LoadFromWebAsync(requestModel.Url + $"?text={requestModel.Speciality}" + $"&area={requestModel.Area}");
            var anchors = doc.DocumentNode.SelectNodes("//a[@href]");

            List<ResponseModel> responseModels = [];

            foreach (HtmlNode link in anchors)
            {
                HtmlAttribute att = link.Attributes["href"];

                if (att.Value.Contains("rabota.by/vacancy/"))
                {
                    responseModels.Add(new ResponseModel { Url = att.Value });
                }
            }

            return Ok(responseModels);
        }
    }
}
