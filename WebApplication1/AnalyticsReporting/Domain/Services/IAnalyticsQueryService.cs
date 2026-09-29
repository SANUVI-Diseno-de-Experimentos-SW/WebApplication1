using WebApplication1.AnalyticsReporting.Application.Dtos;
using WebApplication1.AnalyticsReporting.Domain.Queries;

namespace WebApplication1.AnalyticsReporting.Domain.Services;

public interface IAnalyticsQueryService
{
    Task<DashboardSummaryResponseDto> GetDashboardSummaryAsync(GetDashboardSummaryQuery query);
    Task<FacilitiesAnalyticsResponseDto> GetFacilitiesAnalyticsAsync(GetFacilitiesAnalyticsQuery query);
    Task<HeatmapDataResponseDto> GetFacilityHeatmapDataAsync(GetFacilityHeatmapDataQuery query);
    Task<TopFacilitiesResponseDto> GetTopFacilitiesAsync(GetTopFacilitiesQuery query);
    Task<PdfReportResponseDto> GeneratePdfReportAsync(GeneratePdfReportQuery query);
}