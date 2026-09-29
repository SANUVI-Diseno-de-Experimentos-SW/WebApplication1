namespace WebApplication1.patient_management.Domain;

/// <summary>
/// Comando para dar de alta a un paciente
/// </summary>
public record DischargePatientCommand(
    string PatientId,
    string NurseId
);