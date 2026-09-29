using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using WebApplication1.AnalyticsReporting.Application.Dtos;

namespace WebApplication1.AnalyticsReporting.Application.Internal;

public class PdfReportService
{
    public async Task<byte[]> GenerateFacilitiesReportAsync(
        DashboardSummaryResponseDto summary,
        List<FacilityAnalyticsItemDto> facilities)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new PdfWriter(memoryStream);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);

        // Título
        document.Add(new Paragraph("Reporte de Postas Médicas")
            .SetFontSize(20)
            .SetBold()
            .SetTextAlignment(TextAlignment.CENTER));

        // Fecha
        document.Add(new Paragraph($"Generado: {DateTime.UtcNow:dd/MM/yyyy HH:mm}")
            .SetFontSize(10)
            .SetTextAlignment(TextAlignment.RIGHT));

        // Resumen Global
        document.Add(new Paragraph("Resumen Global")
            .SetFontSize(16)
            .SetBold()
            .SetUnderline());

        document.Add(new Paragraph($"Total de Postas Activas: {summary.TotalActiveFacilities}"));
        document.Add(new Paragraph($"Postas Críticas (Alto Riesgo): {summary.TotalCriticalFacilities}"));
        document.Add(new Paragraph($"Adherencia Global: {summary.GlobalAdherenceRate}%"));

        document.Add(new Paragraph(" "));

        // Lista de postas
        document.Add(new Paragraph("Detalle de Postas")
            .SetFontSize(16)
            .SetBold()
            .SetUnderline());

        // Crear tabla
        var table = new Table(UnitValue.CreatePercentArray(new float[] { 5, 25, 20, 15, 15, 20 }));
        table.SetWidth(UnitValue.CreatePercentValue(100));

        // Cabeceras
        table.AddHeaderCell(new Cell().Add(new Paragraph("N°").SetBold()));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Posta Médica").SetBold()));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Distrito").SetBold()));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Adherencia").SetBold()));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Riesgo").SetBold()));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Pacientes").SetBold()));

        // Filas
        var rowNum = 1;
        foreach (var facility in facilities)
        {
            // Color según nivel de riesgo
            Color rowColor;
            if (facility.RiskLevel == "HIGH")
                rowColor = new DeviceRgb(220, 38, 38);
            else if (facility.RiskLevel == "MEDIUM")
                rowColor = new DeviceRgb(245, 158, 11);
            else
                rowColor = new DeviceRgb(16, 185, 129);

            table.AddCell(new Cell().Add(new Paragraph(rowNum.ToString())));
            table.AddCell(new Cell().Add(new Paragraph(facility.FacilityName ?? "N/A")));
            table.AddCell(new Cell().Add(new Paragraph(facility.DistrictName ?? "N/A")));
            table.AddCell(new Cell().Add(new Paragraph($"{facility.AdherenceRate}%")));
            table.AddCell(new Cell().Add(new Paragraph(facility.RiskLevel ?? "N/A")));
            table.AddCell(new Cell().Add(new Paragraph(facility.TotalPatients.ToString())));

            rowNum++;
        }

        document.Add(table);

        // Footer
        var totalPages = pdf.GetNumberOfPages();
        for (var i = 1; i <= totalPages; i++)
        {
            var pageSize = pdf.GetPage(i).GetPageSize();
            var x = pageSize.GetWidth() / 2;
            var y = 20;

            var footer = new Paragraph($"Página {i} de {totalPages} - Reporte de Postas Médicas")
                .SetFontSize(8)
                .SetFontColor(DeviceRgb.GREEN);

            document.ShowTextAligned(footer, x, y, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
        }

        document.Close();
        return memoryStream.ToArray();
    }
}