using AutoMapper;

using JobFinders.Domain.Entities;
using JobFinders.Domain.Interfaces;
using JobFinders.Domain.Models;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Security.Cryptography;

namespace JobFinders.Api.Controllers
{
    [EnableCors("AllowClient")]
    public class AuthController: Controller
    {
        private readonly IJwtService _jwtService;
        private readonly IUserManager _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;
        private readonly ICryptoService _cryptoService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(IJwtService jwtService, IUserManager userManager, IEmailSender emailSender, IMapper mapper, ICryptoService cryptoService, IUnitOfWork unitOfWork)
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _emailSender = emailSender;
            _mapper = mapper;
            _cryptoService = cryptoService;
            _unitOfWork = unitOfWork;
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

            string code = _userManager.GenerateOneTimeCode();

            try
            {
                var result = await _emailSender.SendEmailAsync(email, "Код подтверждения", code);

                if (result)
                {
                    await _userManager.RegisterUser(email, password, code);
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

            string code = await _userManager.GenerateOneTimeCodeAsync(user!);
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

        [HttpGet]
        public async Task<IActionResult> GetCurrentUser()
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {                
                var email = User?.Claims?.FirstOrDefault(x => x.Type == "email")?.Value;
                return Ok(new { okText = $"Пользователь {email} в системе" });
            }
            else
            {
                return Unauthorized();
            }            
        }

        [HttpPost]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized();
            }

            var dbSet = _unitOfWork.RefreshTokens.GetAll() as DbSet<RefreshToken>;

            var storedToken = await dbSet!
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked);

            if (storedToken == null || storedToken.ExpiryDate < DateTime.UtcNow)
            { 
                return Unauthorized();
            }

            storedToken.IsRevoked = true;

            var userResponse = _mapper.Map<UserResponse>(storedToken.User);

            await CreateTokens(userResponse);

            return Ok();
        }

        private async Task<IActionResult> SignIn(string email, string value, bool isPassword = false)
        {
            if (User is not null && (User!.Identity?.IsAuthenticated ?? false))
            {
                return BadRequest(new { errorText = $"Пользователь {User.Claims.FirstOrDefault(x => x.Type == "email")?.Value} еще в системе" });
            }

            if (!_userManager.TryGetUserByEmail(email, out User? user))
            {
                return BadRequest(new { errorText = "Пользователь не найден" });
            }

            if (isPassword)
            {
                var encryptedPassword = _cryptoService.Encrypt(value);

                if (encryptedPassword != user?.Password) 
                {
                    return Unauthorized(new { errorText = "Неверный пароль" });
                }
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

            var userResponse = _mapper.Map<UserResponse>(user);
            userResponse.Roles = _userManager.GetRoleNames(user);
            userResponse.Email = _cryptoService.Decrypt(user.Email);

            await CreateTokens(userResponse);

            return Ok(userResponse);
        }

        private async Task CreateTokens(UserResponse userResponse)
        {
             var accessToken = _jwtService.GenerateToken(userResponse);
             AppendCookie("access_token", accessToken, DateTime.UtcNow.AddSeconds(60));

             var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
             var refreshExpiry = DateTime.UtcNow.AddDays(7);

             await _unitOfWork.RefreshTokens.CreateAsync(new RefreshToken
             {
                 Token = refreshToken,
                 UserId = userResponse.UserId,
                 ExpiryDate = refreshExpiry
             });

             await _unitOfWork.SaveChangesAsync();

             AppendCookie("refresh_token", refreshToken, refreshExpiry);
        }

        private void AppendCookie(string cookieName, string token, DateTime expire)
        {
            Response.Cookies.Append(cookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = expire,
            });
        }
    }
}
