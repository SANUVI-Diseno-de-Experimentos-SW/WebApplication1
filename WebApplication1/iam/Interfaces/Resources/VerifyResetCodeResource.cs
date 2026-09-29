using System.ComponentModel.DataAnnotations;

namespace WebApplication1.iam.Interfaces.Resources;

/// <summary>
/// Recurso para verificar código de reset
/// </summary>
public class VerifyResetCodeResource
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Code is required")]
    public string Code { get; set; } = string.Empty;
}