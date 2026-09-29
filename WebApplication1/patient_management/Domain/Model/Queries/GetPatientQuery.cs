namespace WebApplication1.patient_management.Domain.Queries;

/// <summary>
/// Query para obtener un paciente por ID
/// </summary>
public record GetPatientQuery(
    string PatientId
);