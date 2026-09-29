using Microsoft.Extensions.Logging;
using WebApplication1.Contexts.IAM.Domain.Models;
using WebApplication1.Contexts.IAM.Domain.Models.Enums;
using WebApplication1.Contexts.IAM.Domain.Models.ValueObjects;
using WebApplication1.Contexts.IAM.Domain.Queries;
using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.Contexts.IAM.Domain.Services;

namespace WebApplication1.Contexts.IAM.Application.Services;

/// <summary>
/// Implementación del servicio de queries de usuarios (Application Layer)
/// </summary>
public class UserQueryService : IUserQueryService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserQueryService> _logger;

    public UserQueryService(
        IUserRepository userRepository,
        ILogger<UserQueryService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User?> GetUserByIdAsync(GetUserByIdQuery query)
    {
        try
        {
            _logger.LogInformation("Buscando usuario por ID: {UserId}", query.UserId);
            
            var userId = new UserId(query.UserId);
            var user = await _userRepository.FindByIdAsync(userId);
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar usuario por ID: {UserId}", query.UserId);
            throw;
        }
    }

    public async Task<User?> GetUserProfileAsync(GetUserProfileQuery query)
    {
        try
        {
            _logger.LogInformation("Obteniendo perfil de usuario: {UserId}", query.UserId);
            
            var userId = new UserId(query.UserId);
            var user = await _userRepository.FindByIdAsync(userId);
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil de usuario: {UserId}", query.UserId);
            throw;
        }
    }

    public async Task<IReadOnlyList<User>> GetAllStaffUsersAsync(GetAllStaffUsersQuery query)
    {
        try
        {
            _logger.LogInformation("Obteniendo todos los usuarios staff (Página: {Page}, Límite: {Limit})", query.Page, query.Limit);
            
            // Obtener Nurses
            var nurses = await _userRepository.FindByRoleAsync(Role.Nurse);
            
            // Obtener Admins
            var admins = await _userRepository.FindByRoleAsync(Role.Admin);
            
            // Combinar y ordenar
            var allStaff = nurses.Concat(admins)
                .OrderBy(u => u.Name)
                .ThenBy(u => u.Lastname)
                .ToList();
            
            // Aplicar paginación
            var pagedStaff = allStaff
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToList();
            
            _logger.LogInformation("Se encontraron {Count} usuarios staff", pagedStaff.Count);
            
            return pagedStaff.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios staff");
            throw;
        }
    }

    public async Task<IReadOnlyList<User>> GetMothersAsync(GetMothersQuery query)
    {
        try
        {
            _logger.LogInformation("Obteniendo madres (Página: {Page}, Límite: {Limit})", query.Page, query.Limit);
            
            // Usar el método paginado del repositorio
            var (items, totalCount) = await _userRepository.GetPagedByRoleAsync(
                Role.Mother,
                query.Page,
                query.Limit
            );
            
            _logger.LogInformation("Se encontraron {TotalCount} madres, mostrando {Count}", totalCount, items.Count);
            
            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener madres");
            throw;
        }
    }

    public async Task<User?> GetUserByEmailAsync(GetUserByEmailQuery query)
    {
        try
        {
            _logger.LogInformation("Buscando usuario por email: {Email}", query.Email);
            
            var email = new Email(query.Email);
            var user = await _userRepository.FindByEmailAsync(email);
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar usuario por email: {Email}", query.Email);
            throw;
        }
    }
}