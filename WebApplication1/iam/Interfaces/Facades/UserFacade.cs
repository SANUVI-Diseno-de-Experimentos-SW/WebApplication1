using WebApplication1.Contexts.IAM.Domain.Commands;
using WebApplication1.Contexts.IAM.Domain.Models;
using WebApplication1.Contexts.IAM.Domain.Queries;
using WebApplication1.Contexts.IAM.Domain.Services;

namespace WebApplication1.iam.Interfaces.Facades;



/// <summary>
/// Fachada de usuarios - Punto de entrada unificado para la capa de presentación
/// </summary>
public class UserFacade
{
    private readonly IUserCommandService _commandService;
    private readonly IUserQueryService _queryService;

    public UserFacade(
        IUserCommandService commandService,
        IUserQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    // ==========================================
    // COMANDOS
    // ==========================================

    public async Task RegisterMotherAsync(RegisterMotherCommand command)
    {
        await _commandService.RegisterMotherAsync(command);
    }

    public async Task CreateStaffUserAsync(CreateStaffUserCommand command)
    {
        await _commandService.CreateStaffUserAsync(command);
    }

    public async Task<string> LoginAsync(LoginUserCommand command)
    {
        return await _commandService.LoginAsync(command);
    }

    public async Task RequestResetCodeAsync(RequestResetCodeCommand command)
    {
        await _commandService.RequestResetCodeAsync(command);
    }

    public async Task ResetPasswordAsync(ResetPasswordCommand command)
    {
        await _commandService.ResetPasswordAsync(command);
    }

    public async Task VerifyResetCodeAsync(VerifyResetCodeCommand command)
    {
        await _commandService.VerifyResetCodeAsync(command);
    }

    // ==========================================
    // QUERIES
    // ==========================================

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        var query = new GetUserByIdQuery(userId);
        return await _queryService.GetUserByIdAsync(query);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var query = new GetUserByEmailQuery(email);
        return await _queryService.GetUserByEmailAsync(query);
    }

    public async Task<IReadOnlyList<User>> GetAllStaffUsersAsync(int page = 1, int limit = 10)
    {
        var query = new GetAllStaffUsersQuery(page, limit);
        return await _queryService.GetAllStaffUsersAsync(query);
    }

    public async Task<IReadOnlyList<User>> GetMothersAsync(int page = 1, int limit = 10)
    {
        var query = new GetMothersQuery(page, limit);
        return await _queryService.GetMothersAsync(query);
    }

    public async Task<User?> GetUserProfileAsync(string userId)
    {
        var query = new GetUserProfileQuery(userId);
        return await _queryService.GetUserProfileAsync(query);
    }
}