namespace WebApplication1.Contexts.IAM.Domain.Commands;

/// <summary>
/// Comando para solicitar código de reset de contraseña
/// </summary>
public record RequestResetCodeCommand(
    string Email
);