

using WebApplication1.patient_management.Domain.ValueObjects;

namespace WebApplication1.Contexts.PatientManagement.Domain.Commands;

/// <summary>
/// Comando para crear la historia clínica inicial
/// </summary>
public record CreateInitialMedicalRecordCommand(
    string PatientId,
    double Weight,
    double Height,
    string MotivoConsulta,
    string Observaciones,
    List<Antecedente>? Antecedentes = null,
    List<string>? Sintomas = null
);