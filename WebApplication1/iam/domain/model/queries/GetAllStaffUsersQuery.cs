namespace WebApplication1.Contexts.IAM.Domain.Queries;

/// <summary>
/// Query para obtener todos los usuarios staff (Nurse y Admin)
/// </summary>
public record GetAllStaffUsersQuery(
    int Page = 1,
    int Limit = 10
);