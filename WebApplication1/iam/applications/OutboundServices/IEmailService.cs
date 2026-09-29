namespace WebApplication1.Contexts.IAM.Application.Interfaces.OutboundServices;

/// <summary>
/// Servicio de envío de emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía un correo con el código de reset de contraseña
    /// </summary>
    Task SendResetCodeAsync(string email, string code);
}