namespace WebApplication1.iam.Interfaces.Resources;

using System.ComponentModel.DataAnnotations;
using WebApplication1.Contexts.IAM.Domain.Models.Enums;
/// <summary>
/// Recurso para crear un usuario staff
/// </summary>
public class CreateStaffUserResource
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lastname is required")]
    [StringLength(100, MinimumLength = 2)]
    public string Lastname { get; set; } = string.Empty;

    [Required(ErrorMessage = "DNI is required")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "DNI must contain exactly 8 numeric digits")]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required")]
    [RegularExpression(@"^(\+51\s)?\d{9}$", ErrorMessage = "Phone must follow format: +51 987654321 or 987654321")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", 
        ErrorMessage = "Password must contain: 8+ characters, one uppercase, one lowercase, and one number")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = string.Empty;
}