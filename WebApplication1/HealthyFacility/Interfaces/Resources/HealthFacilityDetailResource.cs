namespace WebApplication1.HealthyFacility.Interfaces.Resources;

public class HealthFacilityDetailResource
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<string> Services { get; set; } = new();
    public List<string> AvailableDays { get; set; } = new();
    public List<string> AvailableSlots { get; set; } = new();
    public string ScheduleOfOperation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}