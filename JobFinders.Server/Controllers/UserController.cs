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
        private readonly IConfiguration _configuration;

        public UserController(AzureEmailSender emailSender, IConfiguration configuration)
        {
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> SendComment([FromBody] CommentRequest? request)
        {
            if (request is null)
            {
                return BadRequest();
            }

            var email = _configuration["Email"];

            var result = await _emailSender.SendEmailAsync(email,
                    "Отзыв от пользователя JobFinders",
                    $"{request?.Comment}"
                );

            return result ? Ok() : StatusCode(500);
        }
    }
}
