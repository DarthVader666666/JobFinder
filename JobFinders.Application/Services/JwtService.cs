using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using JobFinders.Domain.Entities;
using JobFinders.Domain.Interfaces;
using JobFinders.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JobFinders.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;

        public JwtService(IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            _jwtSettings = new JwtSettings
            {
                Secret = configuration["JwtSecret"],
                Audience = configuration["JwtAudience"],
                Issuer = configuration["JwtIssuer"],
                ExpiryMinutes = int.Parse(configuration["JwtExpiryMinutes"] ?? throw new InvalidOperationException("JwtExpiryMinutes not configured")),
            };
        }

        public string GenerateToken(User? user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret ?? throw new InvalidOperationException("JwtSecret not configured"));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var roles = _unitOfWork.Roles.GetAll().Include(r => r.UserRoles).Where(r => r!.UserRoles!.Select(ur => ur.UserId).Contains(user.UserId)).Select(r => r.RoleName);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role!));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
