using WebApplication1.shared.infrastructure.Middleware;

namespace WebApplication1.shared.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(UserInfo user)
    {
        var jwtSecret = _configuration["JWT_SECRET"] ?? "default-secret-key";
        var key = Encoding.UTF8.GetBytes(jwtSecret);
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new[]
        {
            new Claim("id", user.Id),
            new Claim("motherId", user.MotherId ?? string.Empty),
            new Claim("nurseId", user.NurseId ?? string.Empty),
            new Claim("role", user.Role),
            new Claim("email", user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}