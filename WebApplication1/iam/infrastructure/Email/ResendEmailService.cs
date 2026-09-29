namespace WebApplication1.iam.infrastructure.Email;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApplication1.Contexts.IAM.Application.Interfaces.OutboundServices;


/// <summary>
/// Servicio de email usando Resend API
/// </summary>
public class ResendEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _fromEmail;

    public ResendEmailService(
        IConfiguration configuration,
        ILogger<ResendEmailService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        
        // ✅ Obtener API Key desde configuración
        var apiKey = _configuration["Resend:ApiKey"] 
            ?? throw new InvalidOperationException("Resend ApiKey is required");
        
        // ✅ Obtener email del remitente
        _fromEmail = _configuration["Resend:FromEmail"] 
            ?? "Ferova <onboarding@resend.dev>";
        
        // Configurar HttpClient para Resend
        _httpClient.BaseAddress = new Uri("https://api.resend.com/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        
        _logger.LogInformation("✅ ResendEmailService inicializado con: {FromEmail}", _fromEmail);
    }

    public async Task SendResetCodeAsync(string email, string code)
    {
        try
        {
            _logger.LogInformation("📧 Enviando código de reset a: {Email}", email);

            // Construir el payload de Resend
            var payload = new
            {
                from = _fromEmail,
                to = new[] { email },
                subject = "Ferova - Código de Recuperación de Contraseña",
                html = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset=""utf-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    </head>
                    <body style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
                        <div style=""background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%); padding: 30px; border-radius: 12px; text-align: center;"">
                            <h1 style=""color: white; margin: 0; font-size: 28px;"">Ferova</h1>
                            <p style=""color: #E0E7FF; margin: 5px 0 0;"">Healthcare Management</p>
                        </div>
                        
                        <div style=""background: #F8FAFC; padding: 30px; border-radius: 12px; margin-top: 20px;"">
                            <h2 style=""color: #1E293B; margin-top: 0;"">Recuperación de Contraseña</h2>
                            <p style=""color: #475569; line-height: 1.6;"">
                                Has solicitado restablecer tu contraseña en Ferova.
                                Utiliza el siguiente código de verificación:
                            </p>
                            
                            <div style=""background: white; padding: 20px; border-radius: 8px; text-align: center; border: 2px dashed #4F46E5; margin: 20px 0;"">
                                <span style=""font-size: 36px; font-weight: bold; color: #4F46E5; letter-spacing: 8px;"">
                                    {code}
                                </span>
                            </div>
                            
                            <p style=""color: #64748B; font-size: 14px;"">
                                ⏰ Este código expira en <strong>10 minutos</strong>
                            </p>
                            
                            <hr style=""border: 1px solid #E2E8F0; margin: 20px 0;"">
                            
                            <p style=""color: #94A3B8; font-size: 12px; text-align: center;"">
                                Si no solicitaste este cambio, ignora este mensaje.<br>
                                Este es un correo automático, por favor no responder.
                            </p>
                        </div>
                    </body>
                    </html>
                ",
                text = $@"
                    Ferova - Recuperación de Contraseña
                    
                    Tu código de verificación es: {code}
                    
                    Este código expira en 10 minutos.
                    
                    Si no solicitaste este cambio, ignora este mensaje.
                "
            };

            // Enviar email usando Resend API
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("emails", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Error al enviar email: {Error}", error);
                throw new Exception($"Failed to send email: {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("✅ Email enviado exitosamente a: {Email}. Response: {Response}", email, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al enviar email a: {Email}", email);
            throw new Exception("Could not send reset code email. Please try again later.", ex);
        }
    }
}