
using WebApplication1.iam.Interfaces.Assemblers;
using WebApplication1.iam.Interfaces.Facades;
using WebApplication1.iam.Interfaces.Resources;

namespace WebApplication1.iam.Interfaces;

using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contexts.IAM.Domain.Commands;
using WebApplication1.Contexts.IAM.Domain.Models.Enums;
using WebApplication1.Contexts.IAM.Interfaces.Resources;


/// <summary>
/// Controlador de usuarios
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly UserFacade _userFacade;
    private readonly ILogger<UserController> _logger;

    public UserController(
        UserFacade userFacade,
        ILogger<UserController> logger)
    {
        _userFacade = userFacade;
        _logger = logger;
    }

    // ==========================================
    // 1. REGISTRO DE MADRE
    // ==========================================

    /// <summary>
    /// Registra una nueva madre en el sistema
    /// </summary>
    /// <param name="resource">Datos de la madre</param>
    /// <returns>Mensaje de confirmación</returns>
    [HttpPost("register/mother")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterMother([FromBody] RegisterMotherResource resource)
    {
        try
        {
            // Validar el modelo manualmente (o usar [ApiController] que lo hace automático)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new RegisterMotherCommand(
                resource.Name,
                resource.Lastname,
                resource.Dni,
                resource.Email,
                resource.Phone,
                resource.Password
            );

            await _userFacade.RegisterMotherAsync(command);

            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "Mother registered successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al registrar madre: {Email}", resource.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al registrar madre");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred"
            });
        }
    }

    // ==========================================
    // 2. CREACIÓN DE STAFF
    // ==========================================

    /// <summary>
    /// Crea un usuario staff (Nurse o Admin)
    /// </summary>
    /// <param name="resource">Datos del staff</param>
    /// <returns>Mensaje de confirmación</returns>
    [HttpPost("register/staff")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateStaffUser([FromBody] CreateStaffUserResource resource)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Convertir string a enum Role
            var role = RoleExtensions.FromString(resource.Role);

            var command = new CreateStaffUserCommand(
                resource.Name,
                resource.Lastname,
                resource.Dni,
                resource.Email,
                resource.Phone,
                resource.Password,
                role
            );

            await _userFacade.CreateStaffUserAsync(command);

            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "Staff user registered successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Error al crear usuario staff");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear usuario staff");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred"
            });
        }
    }

    // ==========================================
    // 3. LOGIN
    // ==========================================

    /// <summary>
    /// Inicia sesión en el sistema
    /// </summary>
    /// <param name="resource">Credenciales de login</param>
    /// <returns>Token JWT</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserResource resource)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new LoginUserCommand(
                resource.Dni,
                resource.Password
            );

            var token = await _userFacade.LoginAsync(command);

            return Ok(new { token });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Login fallido para DNI: {Dni}", resource.Dni);
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en login");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred"
            });
        }
    }

    // ==========================================
    // 4. OBTENER USUARIO POR ID
    // ==========================================

    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Datos del usuario</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserById(string id)
    {
        try
        {
            var user = await _userFacade.GetUserByIdAsync(id);

            if (user == null)
                return NotFound(new { error = "User not found" });

            var resource = UserResourceAssembler.ToResource(user);
            return Ok(resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario por ID: {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred"
            });
        }
    }

    // ==========================================
    // 5. OBTENER USUARIO POR EMAIL
    // ==========================================

    /// <summary>
    /// Obtiene un usuario por su email
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <returns>Datos del usuario</returns>
    [HttpGet("email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        try
        {
            var user = await _userFacade.GetUserByEmailAsync(email);

            if (user == null)
                return NotFound(new { error = "User not found" });

            var resource = UserResourceAssembler.ToResource(user);
            return Ok(resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario por email: {Email}", email);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred"
            });
        }
    }

    // ==========================================
    // 6. SOLICITAR CÓDIGO DE RESET
    // ==========================================

    /// <summary>
    /// Solicita un código de reset de contraseña
    /// </summary>
    /// <param name="resource">Email del usuario</param>
    /// <returns>Mensaje de confirmación</returns>
    [HttpPost("password/request-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RequestResetCode([FromBody] RequestResetCodeResource resource)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new RequestResetCodeCommand(resource.Email);
            await _userFacade.RequestResetCodeAsync(command);

            return Ok(new { message = "Reset code sent successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al solicitar código de reset para: {Email}", resource.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al solicitar código de reset");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred"
            });
        }
    }

    // ==========================================
    // 7. VERIFICAR CÓDIGO DE RESET
    // ==========================================

    /// <summary>
    /// Verifica el código de reset de contraseña
    /// </summary>
    /// <param name="resource">Email y código</param>
    /// <returns>Mensaje de confirmación</returns>
    [HttpPost("password/verify-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeResource resource)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new VerifyResetCodeCommand(resource.Email, resource.Code);
            await _userFacade.VerifyResetCodeAsync(command);

            return Ok(new { message = "Code verified successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al verificar código de reset para: {Email}", resource.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al verificar código de reset");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred"
            });
        }
    }

    // ==========================================
    // 8. RESETEAR CONTRASEÑA
    // ==========================================

    /// <summary>
    /// Resetea la contraseña usando el código de verificación
    /// </summary>
    /// <param name="resource">Email, código y nueva contraseña</param>
    /// <returns>Mensaje de confirmación</returns>
    [HttpPost("password/reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordResource resource)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new ResetPasswordCommand(
                resource.Email,
                resource.Code,
                resource.NewPassword
            );

            await _userFacade.ResetPasswordAsync(command);

            return Ok(new { message = "Password reset successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al resetear contraseña para: {Email}", resource.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al resetear contraseña");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred"
            });
        }
    }

    // ==========================================
    // 9. OBTENER TODOS LOS STAFF (OPCIONAL)
    // ==========================================

    /// <summary>
    /// Obtiene todos los usuarios staff (Nurses y Admins)
    /// </summary>
    /// <param name="page">Número de página</param>
    /// <param name="limit">Elementos por página</param>
    /// <returns>Lista de usuarios staff</returns>
    [HttpGet("staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllStaffUsers([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var users = await _userFacade.GetAllStaffUsersAsync(page, limit);
            var resources = UserResourceAssembler.ToResources(users);

            return Ok(new
            {
                data = resources,
                pagination = new
                {
                    page,
                    limit,
                    total = users.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios staff");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred"
            });
        }
    }

    // ==========================================
    // 10. OBTENER TODAS LAS MADRES (OPCIONAL)
    // ==========================================

    /// <summary>
    /// Obtiene todas las madres
    /// </summary>
    /// <param name="page">Número de página</param>
    /// <param name="limit">Elementos por página</param>
    /// <returns>Lista de madres</returns>
    [HttpGet("mothers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMothers([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        try
        {
            var users = await _userFacade.GetMothersAsync(page, limit);
            var resources = UserResourceAssembler.ToResources(users);

            return Ok(new
            {
                data = resources,
                pagination = new
                {
                    page,
                    limit,
                    total = users.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener madres");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred"
            });
        }
    }
}