using Microsoft.AspNetCore.Mvc;

namespace JobFinder.WebApp.Controllers
{
    public class FindJobsController : Controller
    {
        [Route("[controller]/GetJobs")]
        public IActionResult GetList()
        {
            return View();
        }
    }
}
