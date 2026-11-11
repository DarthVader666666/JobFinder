using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using JobFinders.Domain.Interfaces;
using JobFinders.Domain.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JobFinders.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IConfiguration configuration)
        {
            _jwtSettings = new JwtSettings
            {
                Secret = configuration["JwtSecret"],
                Audience = configuration["JwtAudience"],
                Issuer = configuration["JwtIssuer"],
                ExpiryMinutes = int.Parse(configuration["JwtExpiryMinutes"] ?? throw new InvalidOperationException("JwtExpiryMinutes not configured")),
            };
        }

        public string GenerateToken(UserResponse? user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret ?? throw new InvalidOperationException("JwtSecret not configured"));

            var claims = new List<Claim>
            {
                new Claim("id", user.UserId.ToString() ?? string.Empty),
                new Claim("name", user.Name ?? string.Empty),
                new Claim("email", user.Email ?? string.Empty),
            };

            foreach (var role in user.Roles ?? [])
            {
                claims.Add(new Claim("role", role!));
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
