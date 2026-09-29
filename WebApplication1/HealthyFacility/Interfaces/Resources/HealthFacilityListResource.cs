namespace WebApplication1.HealthyFacility.Interfaces.Resources;

public class HealthFacilityListResource
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
}