namespace WebApplication1.Contexts.IAM.Domain.Queries;

/// <summary>
/// Query para obtener usuario por ID
/// </summary>
public record GetUserByIdQuery(
    string UserId
);