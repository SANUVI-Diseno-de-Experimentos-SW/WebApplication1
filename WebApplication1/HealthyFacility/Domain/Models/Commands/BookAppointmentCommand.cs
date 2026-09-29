namespace WebApplication1.HealthyFacility.Domain.Models.Commands;

public record BookAppointmentCommand(
    string FacilityId,
    string PatientId,
    string MotherId,
    string AppointmentDate,
    string AppointmentTime
);