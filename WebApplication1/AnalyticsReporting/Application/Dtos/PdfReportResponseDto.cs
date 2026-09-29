namespace WebApplication1.AnalyticsReporting.Application.Dtos;

public class PdfReportResponseDto
{
    public string PdfBase64 { get; }
    public string FileName { get; }

    public PdfReportResponseDto(string pdfBase64, string fileName)
    {
        PdfBase64 = pdfBase64;
        FileName = fileName;
    }
}