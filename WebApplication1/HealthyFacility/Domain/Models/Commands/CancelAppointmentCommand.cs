namespace WebApplication1.HealthyFacility.Domain.Models.Commands;

public record CancelAppointmentCommand(
    string AppointmentId
);