using JobFinders.BLL.Services;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using JobFinders.Server.Models;

namespace JobFinders.Server.Controllers
{
    [EnableCors("AllowClient")]
    public class UserController : Controller
    {
        private readonly AzureEmailSender _emailSender;

        public UserController(AzureEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [HttpPost]
        public async Task<IActionResult> SendComment([FromBody] CommentRequest? request)
        {
            if (request is null)
            {
                return BadRequest();
            }

            var result = await _emailSender.SendEmailAsync("rumyancer@gmail.com",
                    "Отзыв от пользователя JobFinders",
                    $"{request?.Comment}"
                );

            return result ? Ok() : StatusCode(500);
        }
    }
}
