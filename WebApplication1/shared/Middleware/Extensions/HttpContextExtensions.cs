using WebApplication1.shared.infrastructure.Middleware;

namespace WebApplication1.shared.Extensions;



public static class HttpContextExtensions
{
    public static UserInfo? GetUser(this HttpContext context)
    {
        return context.Items["User"] as UserInfo;
    }
}