namespace WebApplication1.patient_management.Domain.Queries;

/// <summary>
/// Query para obtener el conteo de pacientes activos de una enfermera
/// </summary>
public record GetActivePatientsCountQuery(
    string NurseId
);