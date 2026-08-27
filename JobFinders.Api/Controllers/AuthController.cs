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

        public AuthController(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = _jwtService.GenerateToken(
                userName: request?.Name,
                email: request?.Email,
                roles: new[] { "User" }
            );

            return Ok(new { Token = token });
        }
    }
}
