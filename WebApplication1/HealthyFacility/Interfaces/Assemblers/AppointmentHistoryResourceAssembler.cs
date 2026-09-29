using WebApplication1.HealthyFacility.Interfaces.Resources;

namespace WebApplication1.HealthyFacility.Interfaces.Assemblers;



public static class AppointmentHistoryResourceAssembler
{
    public static AppointmentHistoryResource ToResource(dynamic item)
    {
        var appointmentData = item.appointment.ToPrimitives();

        return new AppointmentHistoryResource
        {
            AppointmentId = appointmentData.Id,
            FacilityName = item.facilityName,
            PatientId = appointmentData.PatientId,
            AppointmentDate = appointmentData.AppointmentDate,
            AppointmentTime = appointmentData.AppointmentTime,
            Status = appointmentData.Status
        };
    }
}