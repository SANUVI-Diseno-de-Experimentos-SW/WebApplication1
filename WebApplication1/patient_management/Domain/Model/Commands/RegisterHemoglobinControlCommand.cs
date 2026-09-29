namespace WebApplication1.patient_management.Domain;

/// <summary>
/// Comando para registrar un control de hemoglobina
/// </summary>
public record RegisterHemoglobinControlCommand(
    string PatientId,
    double HemoglobinLevel
);