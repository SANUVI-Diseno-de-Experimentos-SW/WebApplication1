namespace WebApplication1.HealthyFacility.Domain.Models.Commands;


public record RegisterHealthFacilityCommand(
    string Name,
    string Address,
    string DistrictId,
    double Latitude,
    double Longitude,
    string PhoneNumber,
    List<string> Services,
    List<string> AvailableDays,
    List<string> AvailableSlots
);