namespace WebApplication1.Contexts.IAM.Domain.Commands;

/// <summary>
/// Comando para verificar código de reset
/// </summary>
public record VerifyResetCodeCommand(
    string Email,
    string Code
);