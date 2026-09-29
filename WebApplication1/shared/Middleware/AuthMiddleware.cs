namespace WebApplication1.shared.infrastructure.Middleware;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public AuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                if (!string.IsNullOrEmpty(token))
                {
                    var userInfo = ValidateToken(token);
                    if (userInfo != null)
                    {
                        context.Items["User"] = userInfo;

                        var claims = new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, userInfo.Id),
                            new Claim(ClaimTypes.Email, userInfo.Email),
                            new Claim(ClaimTypes.Role, userInfo.Role),
                            new Claim("MotherId", userInfo.MotherId ?? string.Empty),
                            new Claim("NurseId", userInfo.NurseId ?? string.Empty)
                        };

                        var identity = new ClaimsIdentity(claims, "Bearer");
                        context.User = new ClaimsPrincipal(identity);
                    }
                }
            }

            await _next(context);
        }
        catch
        {
            await _next(context);
        }
    }

    private UserInfo? ValidateToken(string token)
    {
        try
        {
            var jwtSecret = _configuration["JWT_SECRET"] ?? "default-secret-key";
            var key = Encoding.UTF8.GetBytes(jwtSecret);
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;

            var id = jwtToken.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            var motherId = jwtToken.Claims.FirstOrDefault(c => c.Type == "motherId")?.Value;
            var nurseId = jwtToken.Claims.FirstOrDefault(c => c.Type == "nurseId")?.Value;
            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(role) || string.IsNullOrEmpty(email))
                return null;

            return new UserInfo
            {
                Id = id,
                MotherId = motherId,
                NurseId = nurseId,
                Role = role,
                Email = email
            };
        }
        catch
        {
            return null;
        }
    }
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string? MotherId { get; set; }
    public string? NurseId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public static class AuthMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthMiddleware>();
    }
}