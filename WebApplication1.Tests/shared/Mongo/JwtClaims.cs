using System.Text.Json;

namespace WebApplication1.Tests.shared.Mongo;

/// <summary>Lee un claim del payload de un JWT (sin validar la firma; solo para los tests).</summary>
public static class JwtClaims
{
    public static string? Get(string token, string claim)
    {
        var parts = token.Split('.');
        if (parts.Length < 2) return null;
 
        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
 
        var json = JsonDocument.Parse(Convert.FromBase64String(payload)).RootElement;
        return json.FindString(claim);
    }
}
