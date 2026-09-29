using WebApplication1.HealthyFacility.Interfaces.Resources;

namespace WebApplication1.HealthyFacility.Interfaces.Assemblers;



public static class MotherNextAppointmentResourceAssembler
{
    public static MotherNextAppointmentResource ToResource(dynamic item)
    {
        var data = item.appointment.ToPrimitives();

        return new MotherNextAppointmentResource
        {
            AppointmentId = data.Id,
            AppointmentDate = data.AppointmentDate,
            AppointmentTime = data.AppointmentTime,
            PatientId = data.PatientId,
            FacilityName = item.facilityName,
            Status = data.Status
        };
    }
}