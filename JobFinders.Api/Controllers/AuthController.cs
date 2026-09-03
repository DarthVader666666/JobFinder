using JobFinders.Domain.Entities;
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
        public async Task<IActionResult> SignUp([FromHeader(Name = "Email")] string? email, [FromHeader(Name = "Password")] string? password)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(email);
            ArgumentNullException.ThrowIfNullOrEmpty(password);

            if (_userManager.TryGetUserByEmail(email, out _))
            {
                return BadRequest(new { errorText = $"Пользователь {email} уже зарегестрирован" });
            }

            string code = _userManager.GenerateCode();

            try
            {
                var result = await _emailSender.SendEmailAsync(email, "Код подтверждения", code);

                if (result)
                {
                    await _userManager.RegisterUser(email, password);
                }
                else
                { 
                    return BadRequest(new { errorText = "Не удалось отправить код подтверждения" });
                }
            }
            catch(Exception ex)
            {
                return BadRequest(new { errorText = ex.Message.Contains("format") ? "Неверный формат адреса почты" : ex.Message });
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SignInWithCode([FromHeader(Name = "Email")] string email, [FromHeader(Name = "Code")] string code)
        {
            return await SignIn(email, code);
        }

        [HttpPost]
        public async Task<IActionResult> SignInWithPassword([FromHeader(Name = "Email")] string email, [FromHeader(Name = "Password")] string password)
        {
            return await SignIn(email, password, isPassword: true);
        }

        [HttpPost]
        public async Task<IActionResult> SendCode([FromHeader(Name = "Email")] string email)
        {
            if (email is null)
            {
                return BadRequest(new { errorText = "Email is null" });
            }

            if (!_userManager.TryGetUserByEmail(email, out User? user))
            {
                return BadRequest(new { errorText = "Пользователь не найден" });
            }

            string code = await _userManager.GenerateCodeAsync(user!);
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

        private async Task<IActionResult> SignIn(string email, string value, bool isPassword = false)
        {
            if (!_userManager.TryGetUserByEmail(email, out User? user))
            {
                return BadRequest(new { errorText = "Пользователь не найден" });
            }

            if (isPassword && value != user?.Password)
            {
                return Unauthorized(new { errorText = "Неверный пароль" });
            }

            if (!isPassword)
            {
                if (_userManager.IsCodeExpired(user, out var confirmationCode))
                {
                    return BadRequest(new { errorText = "Код подтверждения истёк" });
                }

                if(value != confirmationCode?.Code)
                { 
                    return Unauthorized(new { errorText = "Неверный код" });
                }
            }

            if (!user!.Confirmed)
            {
                await _userManager.ConfirmUser(user);
            }

            var token = _jwtService.GenerateToken(user);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtExpiryMinutes"] ?? "60")),
                Domain = _configuration["Environment"] == "Development" ? null : _configuration["JwtIssuer"]
            };

            Response.Cookies.Append("access_token", token, cookieOptions);

            return Ok();
        }
    }
}
