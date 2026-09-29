using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.shared.Attributes;

public class RequireRoleAttribute : AuthorizeAttribute
{
    public RequireRoleAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
        AuthenticationSchemes = "Bearer";
    }
}