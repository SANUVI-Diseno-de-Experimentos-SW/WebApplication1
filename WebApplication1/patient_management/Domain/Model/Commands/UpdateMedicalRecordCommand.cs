using WebApplication1.patient_management.Domain.ValueObjects;

namespace WebApplication1.patient_management.Domain.Commands;



/// <summary>
/// Comando para actualizar la historia clínica
/// </summary>
public record UpdateMedicalRecordCommand(
    string PatientId,
    double? Weight = null,
    double? Height = null,
    string? MotivoConsulta = null,
    string? Observaciones = null,
    List<Antecedente>? Antecedentes = null,
    List<string>? Sintomas = null
);