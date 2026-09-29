namespace WebApplication1.patient_management.Domain.Queries;


/// <summary>
/// Query para obtener la historia clínica de un paciente
/// </summary>
public record GetMedicalRecordQuery(
    string PatientId
);