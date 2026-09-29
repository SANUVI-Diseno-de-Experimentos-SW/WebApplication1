namespace WebApplication1.patient_management.Domain.Queries;

/// <summary>
/// Query para obtener el resumen de pacientes de una madre
/// </summary>
public record GetMotherPatientsSummaryQuery(
    string MotherId
);