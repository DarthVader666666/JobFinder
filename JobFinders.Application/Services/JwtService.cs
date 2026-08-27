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

        public string GenerateToken(string? userName, string? email, IEnumerable<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret ?? throw new InvalidOperationException("JwtSecret not configured"));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userName),
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
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
