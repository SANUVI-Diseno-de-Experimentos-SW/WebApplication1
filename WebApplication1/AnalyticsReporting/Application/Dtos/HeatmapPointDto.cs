namespace WebApplication1.AnalyticsReporting.Application.Dtos;

public class HeatmapPointDto
{
    public string FacilityId { get; }
    public string FacilityName { get; }
    public double Lat { get; }
    public double Lng { get; }
    public string RiskLevel { get; }
    public double AdherenceRate { get; }

    public HeatmapPointDto(
        string facilityId,
        string facilityName,
        double lat,
        double lng,
        string riskLevel,
        double adherenceRate)
    {
        FacilityId = facilityId;
        FacilityName = facilityName;
        Lat = lat;
        Lng = lng;
        RiskLevel = riskLevel;
        AdherenceRate = adherenceRate;
    }
}

public class HeatmapDataResponseDto
{
    public List<HeatmapPointDto> Points { get; }

    public HeatmapDataResponseDto(List<HeatmapPointDto> points)
    {
        Points = points;
    }
}