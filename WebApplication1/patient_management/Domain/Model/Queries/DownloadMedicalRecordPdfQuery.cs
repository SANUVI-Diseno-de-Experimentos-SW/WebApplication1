namespace WebApplication1.patient_management.Domain.Queries;

/// <summary>
/// Query para descargar la historia clínica completa en PDF
/// </summary>
public record DownloadMedicalRecordPdfQuery(
    string MedicalRecordId
);