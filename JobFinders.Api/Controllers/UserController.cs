using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using JobFinders.Api.Models;
using JobFinders.Domain.Interfaces;

namespace JobFinders.Api.Controllers
{
    [EnableCors("AllowClient")]
    public class UserController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public UserController(IEmailSender emailSender, IConfiguration configuration)
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
                    $"<span style=\"font-size: 1rem\">{request?.Comment}</span>"
                );

            return result ? Ok() : StatusCode(500);
        }
    }
}
