using WebApplication1.Contexts.IAM.Domain.Models.Enums;

namespace WebApplication1.Contexts.IAM.Application.DTOs;

/// <summary>
/// Payload del token JWT
/// </summary>
public record TokenPayload(
    string Id,
    string Email,
    Role Role,
    string? MotherId = null,
    string? NurseId = null
);