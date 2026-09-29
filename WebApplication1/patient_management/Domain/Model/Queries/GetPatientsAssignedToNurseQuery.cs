namespace WebApplication1.patient_management.Domain.Queries;

/// <summary>
/// Query para obtener pacientes asignados a una enfermera
/// </summary>
public record GetPatientsAssignedToNurseQuery(
    string NurseId,
    string? SearchTerm = null
);