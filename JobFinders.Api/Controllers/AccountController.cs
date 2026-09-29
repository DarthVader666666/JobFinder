using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace JobFinders.Api.Controllers
{
    [Authorize]
    [EnableCors("AllowClient")]
    public class AccountController: ControllerBase
    {
        public AccountController()
        {
            
        }

        [HttpGet]
        public async Task<IActionResult> GetAccount()
        {
            return Ok();
        }
    }
}
