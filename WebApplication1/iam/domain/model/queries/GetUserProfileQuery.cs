namespace WebApplication1.Contexts.IAM.Domain.Queries;

/// <summary>
/// Query para obtener el perfil de un usuario
/// </summary>
public record GetUserProfileQuery(
    string UserId
);