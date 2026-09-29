using WebApplication1.AnalyticsReporting.Application.Dtos;
using WebApplication1.AnalyticsReporting.Domain.Queries;
using WebApplication1.AnalyticsReporting.Domain.Services;
using WebApplication1.AnalyticsReporting.Infrastructure.Persitencia.MongoDb.Repositories;

namespace WebApplication1.AnalyticsReporting.Application.Internal;

public class AnalyticsQueryServiceImpl : IAnalyticsQueryService
{
    private readonly MongoAnalyticsRepository _analyticsRepository;
    private readonly PdfReportService _pdfReportService;

    public AnalyticsQueryServiceImpl(MongoAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
        _pdfReportService = new PdfReportService();
    }

    public async Task<DashboardSummaryResponseDto> GetDashboardSummaryAsync(GetDashboardSummaryQuery query)
    {
        return await _analyticsRepository.GetDashboardSummaryAsync();
    }

    public async Task<FacilitiesAnalyticsResponseDto> GetFacilitiesAnalyticsAsync(GetFacilitiesAnalyticsQuery query)
    {
        return await _analyticsRepository.GetFacilitiesAnalyticsAsync(query.RiskLevelFilter);
    }

    public async Task<HeatmapDataResponseDto> GetFacilityHeatmapDataAsync(GetFacilityHeatmapDataQuery query)
    {
        return await _analyticsRepository.GetFacilityHeatmapDataAsync(query.RiskLevelFilter);
    }

    public async Task<TopFacilitiesResponseDto> GetTopFacilitiesAsync(GetTopFacilitiesQuery query)
    {
        return await _analyticsRepository.GetTopFacilitiesAsync();
    }

    public async Task<PdfReportResponseDto> GeneratePdfReportAsync(GeneratePdfReportQuery query)
    {
        var summary = await _analyticsRepository.GetDashboardSummaryAsync();
        var facilitiesResponse = await _analyticsRepository.GetFacilitiesAnalyticsAsync(null);

        var pdfBuffer = await _pdfReportService.GenerateFacilitiesReportAsync(
            summary,
            facilitiesResponse.Facilities
        );

        var pdfBase64 = Convert.ToBase64String(pdfBuffer);

        return new PdfReportResponseDto(
            pdfBase64,
            $"reporte_postas_{DateTime.UtcNow:yyyy-MM-dd}.pdf"
        );
    }
}