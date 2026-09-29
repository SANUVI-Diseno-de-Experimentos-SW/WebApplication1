using WebApplication1.Contexts.IAM.Domain.Commands;

namespace WebApplication1.Contexts.IAM.Domain.Services;

/// <summary>
/// Servicio de comandos para usuarios - Interface de dominio
/// </summary>
public interface IUserCommandService
{
    /// <summary>
    /// Registra una nueva madre en el sistema
    /// </summary>
    Task RegisterMotherAsync(RegisterMotherCommand command);
    
    /// <summary>
    /// Crea un usuario staff (Nurse o Admin)
    /// </summary>
    Task CreateStaffUserAsync(CreateStaffUserCommand command);
    
    /// <summary>
    /// Inicia sesión y retorna un token JWT
    /// </summary>
    Task<string> LoginAsync(LoginUserCommand command);
    
    /// <summary>
    /// Solicita código de reset de contraseña
    /// </summary>
    Task RequestResetCodeAsync(RequestResetCodeCommand command);
    
    /// <summary>
    /// Resetea la contraseña usando el código
    /// </summary>
    Task ResetPasswordAsync(ResetPasswordCommand command);
    
    /// <summary>
    /// Verifica el código de reset de contraseña
    /// </summary>
    Task VerifyResetCodeAsync(VerifyResetCodeCommand command);
}