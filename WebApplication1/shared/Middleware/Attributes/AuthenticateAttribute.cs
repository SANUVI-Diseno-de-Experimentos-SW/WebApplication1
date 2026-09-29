namespace WebApplication1.shared.Attributes;

using Microsoft.AspNetCore.Authorization;


public class AuthenticateAttribute : AuthorizeAttribute
{
    public AuthenticateAttribute()
    {
        AuthenticationSchemes = "Bearer";
    }
}