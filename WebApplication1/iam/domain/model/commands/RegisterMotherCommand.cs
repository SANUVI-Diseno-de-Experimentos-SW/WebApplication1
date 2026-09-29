namespace WebApplication1.Contexts.IAM.Domain.Commands;

/// <summary>
/// Comando para registrar una madre
/// </summary>
public record RegisterMotherCommand(
    string Name,
    string Lastname,
    string Dni,
    string Email,
    string Phone,
    string Password
);