using Microsoft.Extensions.Logging;
using WebApplication1.Contexts.IAM.Application.DTOs;
using WebApplication1.Contexts.IAM.Application.Interfaces.OutboundServices;
using WebApplication1.Contexts.IAM.Domain.Commands;
using WebApplication1.Contexts.IAM.Domain.Models;
using WebApplication1.Contexts.IAM.Domain.Models.Enums;
using WebApplication1.Contexts.IAM.Domain.Models.ValueObjects;
using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.Contexts.IAM.Domain.Services;
using WebApplication1.Contexts.IAM.Infrastructure.Security;
using WebApplication1.iam.infrastructure.tokens;

namespace WebApplication1.Contexts.IAM.Application.Services;

/// <summary>
/// Implementación del servicio de comandos de usuarios (Application Layer)
/// </summary>
public class UserCommandService : IUserCommandService
{
    private readonly IUserRepository _userRepository;
    private readonly IBcryptHashingService _bcryptService;
    private readonly IJwtTokenService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserCommandService> _logger;

    public UserCommandService(
        IUserRepository userRepository,
        IBcryptHashingService bcryptService,
        IJwtTokenService jwtService,
        IEmailService emailService,
        ILogger<UserCommandService> logger)
    {
        _userRepository = userRepository;
        _bcryptService = bcryptService;
        _jwtService = jwtService;
        _emailService = emailService;
        _logger = logger;
    }

    // ==========================================
    // REGISTRO DE MADRE
    // ==========================================

    public async Task RegisterMotherAsync(RegisterMotherCommand command)
    {
        try
        {
            _logger.LogInformation("Registrando nueva madre: {Email}", command.Email);

            // Validar que no exista por Email
            var email = new Email(command.Email);
            var existingUser = await _userRepository.FindByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogWarning("Intento de registro con email existente: {Email}", command.Email);
                throw new InvalidOperationException("User already exists");
            }

            // Hashear contraseña
            var hashedPassword = await _bcryptService.HashAsync(command.Password);

            // Crear usuario
            var user = new User(
                new UserId(Guid.NewGuid().ToString()),
                command.Name,
                command.Lastname,
                new Password(hashedPassword),
                Role.Mother,
                new Dni(command.Dni),
                new Email(command.Email),
                new Phone(command.Phone)
            );

            await _userRepository.SaveAsync(user);
            
            _logger.LogInformation("Madre registrada exitosamente: {Email}", command.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar madre: {Email}", command.Email);
            throw;
        }
    }

    // ==========================================
    // CREACIÓN DE STAFF
    // ==========================================

    public async Task CreateStaffUserAsync(CreateStaffUserCommand command)
    {
        try
        {
            _logger.LogInformation("Creando usuario staff: {Email}, Rol: {Role}", command.Email, command.Role);

            // Validar rol
            if (command.Role != Role.Nurse && command.Role != Role.Admin)
            {
                throw new ArgumentException("Invalid staff role. Must be Nurse or Admin");
            }

            // Validar que no exista por Email
            var email = new Email(command.Email);
            var existingUser = await _userRepository.FindByEmailAsync(email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Validar que no exista por DNI
            var dni = new Dni(command.Dni);
            var existingByDni = await _userRepository.FindByDniAsync(dni);
            if (existingByDni != null)
            {
                throw new InvalidOperationException("User with this DNI already exists");
            }

            // Hashear contraseña
            var hashedPassword = await _bcryptService.HashAsync(command.Password);

            // Crear usuario
            var user = new User(
                new UserId(Guid.NewGuid().ToString()),
                command.Name,
                command.Lastname,
                new Password(hashedPassword),
                command.Role,
                new Dni(command.Dni),
                new Email(command.Email),
                new Phone(command.Phone)
            );

            await _userRepository.SaveAsync(user);
            
            _logger.LogInformation("Usuario staff creado exitosamente: {Email}, Rol: {Role}", command.Email, command.Role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario staff: {Email}", command.Email);
            throw;
        }
    }

    // ==========================================
    // LOGIN
    // ==========================================

    public async Task<string> LoginAsync(LoginUserCommand command)
    {
        try
        {
            _logger.LogInformation("Intento de login: {Dni}", command.Dni);

            // Buscar por DNI
            var dni = new Dni(command.Dni);
            var user = await _userRepository.FindByDniAsync(dni);

            if (user == null)
            {
                _logger.LogWarning("Login fallido - Usuario no encontrado: {Dni}", command.Dni);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Verificar contraseña
            var isPasswordValid = await _bcryptService.CompareAsync(
                command.Password,
                user.Password.Value
            );

            if (!isPasswordValid)
            {
                _logger.LogWarning("Login fallido - Contraseña incorrecta: {Dni}", command.Dni);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Verificar si está activo
            if (!user.IsActive)
            {
                _logger.LogWarning("Login fallido - Usuario inactivo: {Dni}", command.Dni);
                throw new UnauthorizedAccessException("User is inactive");
            }

            // Construir payload del token
            var userId = user.Id.Value;
            var role = user.Role;
            string? motherId = null;
            string? nurseId = null;

            if (role == Role.Mother)
                motherId = userId;
            else if (role == Role.Nurse)
                nurseId = userId;
            // Admin: ambos null

            var tokenPayload = new TokenPayload(
                userId,
                user.Email.Value,
                role,
                motherId,
                nurseId
            );

            // Generar token
            var token = _jwtService.GenerateToken(tokenPayload);
            
            _logger.LogInformation("Login exitoso: {Dni}, Rol: {Role}", command.Dni, role);
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante login: {Dni}", command.Dni);
            throw;
        }
    }

    // ==========================================
    // RESET DE CONTRASEÑA
    // ==========================================

    public async Task RequestResetCodeAsync(RequestResetCodeCommand command)
    {
        try
        {
            _logger.LogInformation("Solicitando código de reset: {Email}", command.Email);

            var email = new Email(command.Email);
            var user = await _userRepository.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para reset: {Email}", command.Email);
                throw new InvalidOperationException("User not found");
            }

            // Generar código de 4 dígitos
            var random = new Random();
            var code = random.Next(1000, 9999).ToString();

            // Expira en 10 minutos
            var expiresAt = DateTime.UtcNow.AddMinutes(10);

            // Guardar código
            await _userRepository.SaveResetCodeAsync(email, code, expiresAt);

            // Enviar email
            await _emailService.SendResetCodeAsync(command.Email, code);

            _logger.LogInformation("Código de reset enviado a: {Email}", command.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al solicitar código de reset: {Email}", command.Email);
            throw;
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordCommand command)
    {
        try
        {
            _logger.LogInformation("Reseteando contraseña: {Email}", command.Email);

            var email = new Email(command.Email);

            // Verificar que el código sea válido
            var isValid = await _userRepository.ValidateResetCodeAsync(email, command.Code);
            if (!isValid)
            {
                _logger.LogWarning("Código de reset inválido o expirado: {Email}", command.Email);
                throw new InvalidOperationException("Invalid or expired code");
            }

            // Hashear nueva contraseña
            var hashedPassword = await _bcryptService.HashAsync(command.NewPassword);

            // Actualizar contraseña
            await _userRepository.UpdatePasswordAsync(email, hashedPassword);

            // Limpiar código de reset
            await _userRepository.ClearResetCodeAsync(email);

            _logger.LogInformation("Contraseña resetada exitosamente: {Email}", command.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al resetear contraseña: {Email}", command.Email);
            throw;
        }
    }

    public async Task VerifyResetCodeAsync(VerifyResetCodeCommand command)
    {
        try
        {
            _logger.LogInformation("Verificando código de reset: {Email}", command.Email);

            var email = new Email(command.Email);
            var isValid = await _userRepository.ValidateResetCodeAsync(email, command.Code);

            if (!isValid)
            {
                _logger.LogWarning("Código de reset inválido: {Email}", command.Email);
                throw new InvalidOperationException("Invalid or expired code");
            }

            _logger.LogInformation("Código de reset verificado exitosamente: {Email}", command.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar código de reset: {Email}", command.Email);
            throw;
        }
    }
}