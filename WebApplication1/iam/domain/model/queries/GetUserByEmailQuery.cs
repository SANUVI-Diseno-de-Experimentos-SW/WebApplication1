namespace WebApplication1.Contexts.IAM.Domain.Queries;

/// <summary>
/// Query para obtener usuario por email
/// </summary>
public record GetUserByEmailQuery(
    string Email
);