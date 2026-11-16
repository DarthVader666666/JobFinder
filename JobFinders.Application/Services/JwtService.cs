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
            _jwtSettings = new JwtSettings
            {
                Secret = configuration["JwtSecret"],
                Audience = configuration["JwtAudience"],
                Issuer = configuration["JwtIssuer"],
                ExpiryMinutes = int.Parse(configuration["AccessTokenMinutes"] ?? throw new InvalidOperationException("AccessTokenMinutes not configured")),
            };

            _unitOfWork = unitOfWork;
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

        public async Task RevokeRefreshToken(int userId)
        {
            var refreshTokens = _unitOfWork.RefreshTokens.GetAll() as DbSet<RefreshToken> ?? throw new NullReferenceException();

            var tokens = await refreshTokens
                .Where(t => t.UserId == userId && t.IsRevoked == false)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
