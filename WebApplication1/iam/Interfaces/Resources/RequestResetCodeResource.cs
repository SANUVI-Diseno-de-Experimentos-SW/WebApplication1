using System.ComponentModel.DataAnnotations;

namespace WebApplication1.iam.Interfaces.Resources;

public class RequestResetCodeResource
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}