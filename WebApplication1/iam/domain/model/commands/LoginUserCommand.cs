namespace WebApplication1.Contexts.IAM.Domain.Commands;

/// <summary>
/// Comando para login de usuario
/// </summary>
public record LoginUserCommand(
    string Dni,
    string Password
);