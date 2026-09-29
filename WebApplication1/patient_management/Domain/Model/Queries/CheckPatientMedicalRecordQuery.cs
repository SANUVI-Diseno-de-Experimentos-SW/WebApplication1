namespace WebApplication1.patient_management.Domain.Queries;

/// <summary>
/// Query para verificar si un paciente tiene historia clínica
/// </summary>
public record CheckPatientMedicalRecordQuery(
    string PatientId
);