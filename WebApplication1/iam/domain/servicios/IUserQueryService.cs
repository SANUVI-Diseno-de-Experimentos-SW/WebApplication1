using WebApplication1.Contexts.IAM.Domain.Models;
using WebApplication1.Contexts.IAM.Domain.Queries;

namespace WebApplication1.Contexts.IAM.Domain.Services;

/// <summary>
/// Servicio de queries para usuarios - Interface de dominio
/// </summary>
public interface IUserQueryService
{
    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    Task<User?> GetUserByIdAsync(GetUserByIdQuery query);
    
    /// <summary>
    /// Obtiene el perfil completo de un usuario
    /// </summary>
    Task<User?> GetUserProfileAsync(GetUserProfileQuery query);
    
    /// <summary>
    /// Obtiene todos los usuarios staff (Nurses y Admins)
    /// </summary>
    Task<IReadOnlyList<User>> GetAllStaffUsersAsync(GetAllStaffUsersQuery query);
    
    /// <summary>
    /// Obtiene todas las madres
    /// </summary>
    Task<IReadOnlyList<User>> GetMothersAsync(GetMothersQuery query);
    
    /// <summary>
    /// Obtiene un usuario por su email
    /// </summary>
    Task<User?> GetUserByEmailAsync(GetUserByEmailQuery query);
}