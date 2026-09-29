using WebApplication1.HealthyFacility.Domain.Models.Commands;

namespace WebApplication1.HealthyFacility.Domain.Services;



public interface IHealthyFacilityCommandService
{
    Task BookAppointmentAsync(BookAppointmentCommand command);
    Task AssignNurseToFacilityAsync(AssignNurseToFacilityCommand command);
    Task CancelAppointmentAsync(CancelAppointmentCommand command);
    Task RegisterFacilityAsync(RegisterHealthFacilityCommand command);
    Task ValidateAppointmentBelongsToMotherAsync(string appointmentId, string motherId);
    Task ValidatePatientBelongsToMotherAsync(string patientId, string motherId);
}