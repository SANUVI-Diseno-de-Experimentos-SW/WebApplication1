namespace WebApplication1.patient_management.Domain.Queries;

/// <summary>
/// Query para descargar el reporte de hemoglobina en PDF
/// </summary>
public record DownloadHemoglobinReportPdfQuery(
    string MedicalRecordId
);