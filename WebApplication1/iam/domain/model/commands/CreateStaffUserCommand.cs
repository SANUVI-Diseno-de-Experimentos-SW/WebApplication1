using WebApplication1.Contexts.IAM.Domain.Models.Enums;

namespace WebApplication1.Contexts.IAM.Domain.Commands;

/// <summary>
/// Comando para crear usuario staff (Nurse o Admin)
/// </summary>
public record CreateStaffUserCommand(
    string Name,
    string Lastname,
    string Dni,
    string Email,
    string Phone,
    string Password,
    Role Role
);