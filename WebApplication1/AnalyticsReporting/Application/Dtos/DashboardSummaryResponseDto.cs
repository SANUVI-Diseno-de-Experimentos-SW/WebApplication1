namespace WebApplication1.AnalyticsReporting.Application.Dtos;

public class DashboardSummaryResponseDto
{
    public int TotalActiveFacilities { get; }
    public int TotalCriticalFacilities { get; }
    public double GlobalAdherenceRate { get; }

    public DashboardSummaryResponseDto(
        int totalActiveFacilities,
        int totalCriticalFacilities,
        double globalAdherenceRate)
    {
        TotalActiveFacilities = totalActiveFacilities;
        TotalCriticalFacilities = totalCriticalFacilities;
        GlobalAdherenceRate = globalAdherenceRate;
    }
}