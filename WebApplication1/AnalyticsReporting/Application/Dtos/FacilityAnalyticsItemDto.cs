namespace WebApplication1.AnalyticsReporting.Application.Dtos;

public class FacilityAnalyticsItemDto
{
    public string FacilityId { get; }
    public string FacilityName { get; }
    public string DistrictName { get; }
    public double AdherenceRate { get; }
    public string RiskLevel { get; }
    public int TotalPatients { get; }
    public int TotalConfirmed { get; }
    public int TotalOmitted { get; }

    public FacilityAnalyticsItemDto(
        string facilityId,
        string facilityName,
        string districtName,
        double adherenceRate,
        string riskLevel,
        int totalPatients,
        int totalConfirmed,
        int totalOmitted)
    {
        FacilityId = facilityId;
        FacilityName = facilityName;
        DistrictName = districtName;
        AdherenceRate = adherenceRate;
        RiskLevel = riskLevel;
        TotalPatients = totalPatients;
        TotalConfirmed = totalConfirmed;
        TotalOmitted = totalOmitted;
    }
}

public class FacilitiesAnalyticsResponseDto
{
    public List<FacilityAnalyticsItemDto> Facilities { get; }

    public FacilitiesAnalyticsResponseDto(List<FacilityAnalyticsItemDto> facilities)
    {
        Facilities = facilities;
    }
}