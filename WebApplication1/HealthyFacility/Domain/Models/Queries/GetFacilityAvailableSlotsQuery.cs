namespace WebApplication1.HealthyFacility.Domain.Models.Queries;

public record GetFacilityAvailableSlotsQuery(
    string FacilityId,
    string AppointmentDate
);