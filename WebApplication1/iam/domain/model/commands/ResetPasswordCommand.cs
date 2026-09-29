namespace WebApplication1.Contexts.IAM.Domain.Commands;

/// <summary>
/// Comando para resetear contraseña con código
/// </summary>
public record ResetPasswordCommand(
    string Email,
    string Code,
    string NewPassword
);