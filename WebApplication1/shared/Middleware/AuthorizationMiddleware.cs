using WebApplication1.shared.Extensions;

namespace WebApplication1.shared.infrastructure.Middleware;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


/// <summary>
/// Middleware para verificar roles (alternativa a [Authorize(Roles = "...")])
/// </summary>
public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string[] _allowedRoles;

    public AuthorizationMiddleware(RequestDelegate next, params string[] allowedRoles)
    {
        _next = next;
        _allowedRoles = allowedRoles;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.GetUser();

        if (user == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "No autenticado" });
            return;
        }

        if (!_allowedRoles.Contains(user.Role))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new
            {
                error = $"Acceso denegado. Rol requerido: {string.Join(" o ", _allowedRoles)}"
            });
            return;
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods para autorización
/// </summary>
public static class AuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder RequireRole(this IApplicationBuilder builder, params string[] roles)
    {
        return builder.UseMiddleware<AuthorizationMiddleware>(roles);
    }

    public static IApplicationBuilder RequireMother(this IApplicationBuilder builder)
    {
        return builder.RequireRole("Mother");
    }

    public static IApplicationBuilder RequireNurse(this IApplicationBuilder builder)
    {
        return builder.RequireRole("Nurse");
    }

    public static IApplicationBuilder RequireAdmin(this IApplicationBuilder builder)
    {
        return builder.RequireRole("Admin");
    }

    public static IApplicationBuilder RequireMotherOrNurse(this IApplicationBuilder builder)
    {
        return builder.RequireRole("Mother", "Nurse");
    }
}