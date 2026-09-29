namespace WebApplication1.iam.Interfaces.Resources;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Recurso para resetear contraseña
/// </summary>
public class ResetPasswordResource
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Code is required")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [MinLength(8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$", 
        ErrorMessage = "Password must contain uppercase, lowercase, number and symbol")]
    public string NewPassword { get; set; } = string.Empty;
}