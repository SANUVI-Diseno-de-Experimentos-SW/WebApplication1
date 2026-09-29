using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using WebApplication1.patient_management.Domain.Aggregate;
using WebApplication1.patient_management.Domain.Entities;

namespace WebApplication1.patient_management.Infrastructure.Services;

public static class PdfService
{
    public static async Task<byte[]> GenerateMedicalRecordPdfAsync(Patient patient, MedicalRecord medicalRecord)
    {
        if (patient == null)
            throw new ArgumentNullException(nameof(patient));
        
        if (medicalRecord == null)
            throw new ArgumentNullException(nameof(medicalRecord));

        var patientData = patient.ToPrimitives();
        var medicalData = medicalRecord.ToPrimitives();

        using var memoryStream = new MemoryStream();
        var writer = new PdfWriter(memoryStream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf);

        // Título
        document.Add(new Paragraph("HISTORIA CLÍNICA")
            .SetFontSize(20)
            .SetBold()
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
        
        document.Add(new Paragraph($"Fecha de generación: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC")
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
        
        document.Add(new Paragraph(" "));

        // Datos básicos
        document.Add(new Paragraph($"Paciente: {patientData.Name} {patientData.LastName}"));
        document.Add(new Paragraph($"ID: {patientData.Id}"));
        document.Add(new Paragraph($"Fecha de nacimiento: {patientData.BirthDate:dd/MM/yyyy}"));
        document.Add(new Paragraph($"Peso: {patientData.CurrentWeight:F2} kg"));
        document.Add(new Paragraph($"Altura: {patientData.CurrentHeight:F2} cm"));
        
        document.Add(new Paragraph(" "));
        document.Add(new Paragraph($"Motivo de consulta: {medicalData.MotivoConsulta}"));
        document.Add(new Paragraph($"Observaciones: {(string.IsNullOrEmpty(medicalData.Observaciones) ? "Ninguna" : medicalData.Observaciones)}"));

        if (medicalData.HemoglobinLevel.HasValue)
        {
            document.Add(new Paragraph($"Hemoglobina actual: {medicalData.HemoglobinLevel.Value:F2} g/dL"));
        }

        document.Close();
        pdf.Close();
        writer.Close();

        return memoryStream.ToArray();
    }

    public static async Task<byte[]> GenerateHemoglobinReportPdfAsync(MedicalRecord medicalRecord)
    {
        if (medicalRecord == null)
            throw new ArgumentNullException(nameof(medicalRecord));

        var medicalData = medicalRecord.ToPrimitives();

        using var memoryStream = new MemoryStream();
        var writer = new PdfWriter(memoryStream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf);

        document.Add(new Paragraph("REPORTE DE HEMOGLOBINA")
            .SetFontSize(20)
            .SetBold()
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
        
        document.Add(new Paragraph($"Fecha de generación: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC")
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

        var controls = medicalData.Controls.OrderByDescending(c => c.Date).ToList();

        if (controls.Any())
        {
            foreach (var control in controls)
            {
                document.Add(new Paragraph($"Fecha: {control.Date:dd/MM/yyyy HH:mm}"));
                document.Add(new Paragraph($"Nivel: {(control.HemoglobinLevel.HasValue ? $"{control.HemoglobinLevel.Value:F2} g/dL" : "N/A")}"));
                document.Add(new Paragraph(" "));
            }
        }
        else
        {
            document.Add(new Paragraph("No hay controles de hemoglobina registrados."));
        }

        document.Close();
        pdf.Close();
        writer.Close();

        return memoryStream.ToArray();
    }
}