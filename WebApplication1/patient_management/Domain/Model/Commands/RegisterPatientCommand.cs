namespace WebApplication1.patient_management.Domain;

/// <summary>
/// Comando para registrar un nuevo paciente
/// Estado Inicial: ACTIVE, nurseId = null, facilityId = null, medicalRecord = null
/// </summary>
public record RegisterPatientCommand(
    string Name,
    string LastName,
    DateTime BirthDate,
    string Gender,
    double Weight,
    double Height,
    string MotherId
);