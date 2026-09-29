namespace WebApplication1.patient_management.Domain.Queries;

/// <summary>
/// Query para obtener información básica de un paciente
/// </summary>
public record GetPatientBasicInfoQuery(
    string PatientId
);