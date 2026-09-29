using WebApplication1.Contexts.IAM.Domain.Models.Enums;

namespace WebApplication1.iam.infrastructure.tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Contexts.IAM.Application.DTOs;


/// <summary>
/// Servicio de generación de tokens JWT
/// </summary>
public interface IJwtTokenService
{
    string GenerateToken(TokenPayload payload);
}

/// <summary>
/// Implementación de JWT Token Service
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(TokenPayload payload)
    {
        // ✅ OBTENER LA CLAVE Y VALIDAR SU LONGITUD
        var jwtSecret = _configuration["JWT_SECRET"] ?? "default-secret-key-change-this-in-production";

        // ✅ ASEGURAR QUE LA CLAVE TENGA AL MENOS 32 CARACTERES (256 bits)
        if (jwtSecret.Length < 32)
        {
            // Si es menor a 32, la extendemos con padding
            jwtSecret = jwtSecret.PadRight(32, '!');
            Console.WriteLine($"⚠️ JWT Secret extendido a 32 caracteres para cumplir con HMAC-SHA256");
        }

        var key = Encoding.UTF8.GetBytes(jwtSecret);
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new List<Claim>
        {
            new Claim("id", payload.Id),
            new Claim("email", payload.Email),
            new Claim("role", payload.Role.ToStringValue()),
            new Claim(ClaimTypes.NameIdentifier, payload.Id),
            new Claim(ClaimTypes.Email, payload.Email),
            new Claim(ClaimTypes.Role, payload.Role.ToStringValue())
        };

        // Agregar motherId o nurseId si existen
        if (!string.IsNullOrEmpty(payload.MotherId))
        {
            claims.Add(new Claim("motherId", payload.MotherId));
        }

        if (!string.IsNullOrEmpty(payload.NurseId))
        {
            claims.Add(new Claim("nurseId", payload.NurseId));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = _configuration["JWT_ISSUER"] ?? "ferova-api",
            Audience = _configuration["JWT_AUDIENCE"] ?? "ferova-client",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}