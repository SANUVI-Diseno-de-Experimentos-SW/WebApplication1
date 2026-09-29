using WebApplication1.Contexts.IAM.Domain.Models;
using WebApplication1.Contexts.IAM.Domain.Models.Enums;
using WebApplication1.Contexts.IAM.Domain.Models.ValueObjects;

namespace WebApplication1.Contexts.IAM.Domain.Repositories;

/// <summary>
/// Repositorio de usuarios - Interface de dominio
/// </summary>
public interface IUserRepository
{
    // ==========================================
    // CRUD BÁSICO
    // ==========================================
    
    /// <summary>
    /// Guarda un usuario (crea o actualiza)
    /// </summary>
    Task<User> SaveAsync(User user);
    
    /// <summary>
    /// Busca usuario por ID
    /// </summary>
    Task<User?> FindByIdAsync(UserId id);
    
    /// <summary>
    /// Busca usuario por Email
    /// </summary>
    Task<User?> FindByEmailAsync(Email email);
    
    /// <summary>
    /// Busca usuario por DNI
    /// </summary>
    Task<User?> FindByDniAsync(Dni dni);
    
    /// <summary>
    /// Busca usuario por Teléfono
    /// </summary>
    Task<User?> FindByPhoneAsync(Phone phone);
    
    /// <summary>
    /// Busca usuarios por Rol
    /// </summary>
    Task<IReadOnlyList<User>> FindByRoleAsync(Role role);
    
    /// <summary>
    /// Obtiene todos los usuarios
    /// </summary>
    Task<IReadOnlyList<User>> FindAllAsync();
    
    // ==========================================
    // MÉTODOS PARA BÚSQUEDAS ESPECÍFICAS
    // ==========================================
    
    /// <summary>
    /// Busca madre por DNI
    /// </summary>
    Task<User?> FindMotherByDniAsync(string dni);
    
    /// <summary>
    /// Busca enfermera por ID
    /// </summary>
    Task<User?> FindNurseByIdAsync(string id);
    
    /// <summary>
    /// Busca madre por ID
    /// </summary>
    Task<User?> FindMotherByIdAsync(string id);
    
    /// <summary>
    /// Obtiene todas las enfermeras
    /// </summary>
    Task<IReadOnlyList<User>> FindAllNursesAsync();
    
    /// <summary>
    /// Busca madres por término de búsqueda
    /// </summary>
    Task<IReadOnlyList<User>> FindMothersBySearchTermAsync(string searchTerm);
    
    // ==========================================
    // MÉTODOS PARA RESET DE CONTRASEÑA
    // ==========================================
    
    /// <summary>
    /// Guarda código de reset de contraseña
    /// </summary>
    Task SaveResetCodeAsync(Email email, string code, DateTime expiresAt);
    
    /// <summary>
    /// Valida código de reset de contraseña
    /// </summary>
    Task<bool> ValidateResetCodeAsync(Email email, string code);
    
    /// <summary>
    /// Actualiza contraseña del usuario
    /// </summary>
    Task UpdatePasswordAsync(Email email, string newPassword);
    
    /// <summary>
    /// Limpia código de reset de contraseña
    /// </summary>
    Task ClearResetCodeAsync(Email email);
    
    // ==========================================
    // MÉTODOS ADICIONALES
    // ==========================================
    
    /// <summary>
    /// Obtiene usuarios paginados por rol
    /// </summary>
    Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedByRoleAsync(
        Role role, 
        int page, 
        int pageSize
    );
    
    /// <summary>
    /// Verifica si existe un usuario con el email
    /// </summary>
    Task<bool> ExistsByEmailAsync(Email email);
    
    /// <summary>
    /// Verifica si existe un usuario con el DNI
    /// </summary>
    Task<bool> ExistsByDniAsync(Dni dni);
}