namespace WebApplication1.patient_management.Domain.Queries;

/// <summary>
/// Query para obtener el historial de controles de hemoglobina
/// </summary>
public record GetHemoglobinControlsHistoryQuery(
    string MedicalRecordId
);