using Microsoft.Extensions.Logging;

namespace WebApplication1.Contexts.IAM.Infrastructure.Security;

/// <summary>
/// Servicio de hashing usando BCrypt
/// </summary>
public interface IBcryptHashingService
{
    Task<string> HashAsync(string password);
    Task<bool> CompareAsync(string password, string hashedPassword);
}

/// <summary>
/// Implementación de BCrypt Hashing Service
/// </summary>
public class BcryptHashingService : IBcryptHashingService
{
    private readonly ILogger<BcryptHashingService> _logger;

    public BcryptHashingService(ILogger<BcryptHashingService> logger)
    {
        _logger = logger;
    }

    public Task<string> HashAsync(string password)
    {
        try
        {
            // BCrypt.Net-Next NuGet package
            var hashed = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
            return Task.FromResult(hashed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al hashear contraseña");
            throw;
        }
    }

    public Task<bool> CompareAsync(string password, string hashedPassword)
    {
        try
        {
            var isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            return Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al comparar contraseñas");
            throw;
        }
    }
}