namespace WebApplication1.Contexts.IAM.Domain.Queries;

/// <summary>
/// Query para obtener todas las madres
/// </summary>
public record GetMothersQuery(
    int Page = 1,
    int Limit = 10
);