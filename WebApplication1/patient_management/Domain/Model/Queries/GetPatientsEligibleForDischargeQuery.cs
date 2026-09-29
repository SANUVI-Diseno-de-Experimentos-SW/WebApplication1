namespace WebApplication1.patient_management.Domain.Queries;

/// <summary>
/// Query para obtener pacientes elegibles para alta
/// </summary>
public record GetPatientsEligibleForDischargeQuery(
    string NurseId,
    string? SearchTerm = null
);