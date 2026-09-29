namespace WebApplication1.iam.Interfaces.Resources;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Recurso para login de usuario
/// </summary>
public class LoginUserResource
{
    [Required(ErrorMessage = "DNI is required")]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}