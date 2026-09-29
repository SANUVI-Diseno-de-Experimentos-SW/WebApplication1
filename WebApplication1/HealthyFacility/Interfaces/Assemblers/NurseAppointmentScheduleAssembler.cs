using WebApplication1.HealthyFacility.Domain.Models.Entities;
using WebApplication1.HealthyFacility.Interfaces.Resources;

namespace WebApplication1.HealthyFacility.Interfaces.Assemblers;


public static class NurseAppointmentScheduleAssembler
{
    public static NurseAppointmentScheduleResource ToResource(Appointment appointment, string patientName)
    {
        var data = appointment.ToPrimitives();

        return new NurseAppointmentScheduleResource
        {
            AppointmentId = data.Id,
            PatientId = data.PatientId,
            PatientName = patientName,
            AppointmentDate = data.AppointmentDate,
            AppointmentTime = data.AppointmentTime,
            Status = data.Status
        };
    }
}