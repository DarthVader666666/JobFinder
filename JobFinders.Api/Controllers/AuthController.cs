using JobFinders.Api.Models;
using JobFinders.Domain.Interfaces;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace JobFinders.Api.Controllers
{
    [EnableCors("AllowClient")]
    public class AuthController: Controller
    {
        private readonly IJwtService _jwtService;
        private readonly IUserManager _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AuthController(IJwtService jwtService, IUserManager userManager, IEmailSender emailSender, IConfiguration configuration)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromHeader(Name = "Authorization-Code")] string code)
        {
            if (_userManager.CodeExpired(code))
            {
                return BadRequest(new { errorText = "Код подтверждения истёк" });
            }

            var user = _userManager.GetUserByCode(code);

            if (user is null)
            {
                return Unauthorized(new { errorText = "Неверный код" });
            }

            var token = _jwtService.GenerateToken(user);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15),
                Domain = _configuration["JwtIssuer"]
            };

            Response.Cookies.Append("access_token", token, cookieOptions);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendCode([FromHeader(Name = "Email")] string email)
        {
            if (email is null)
            {
                return BadRequest(new { errorText = "Email is null" });
            }

            string code = await _userManager.GenerateCodeAsync(email);
            var result = await _emailSender.SendEmailAsync(email, "Код подтверждения", code);

            if (result)
            {
                return Ok();
            }
            else
            {
                return StatusCode(500, new { errorText = "Could Not send code" });
            }
        }
    }
}
