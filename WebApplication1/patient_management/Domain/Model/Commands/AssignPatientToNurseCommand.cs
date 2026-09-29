namespace WebApplication1.patient_management.Domain;

/// <summary>
/// Comando para asignar un paciente a una enfermera
/// Automáticamente obtenemos el facilityId de la enfermera
/// </summary>
public record AssignPatientToNurseCommand(
    string PatientId,
    string NurseId
);