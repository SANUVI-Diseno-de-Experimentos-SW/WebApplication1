using WebApplication1.HealthyFacility.Domain.Models.Entities;
using WebApplication1.HealthyFacility.Domain.Models.ValueObjects;

namespace WebApplication1.HealthyFacility.Infrastructure.Mappers;


public static class AppointmentMapper
{
    public static Appointment ToDomain(dynamic document)
    {
        return new Appointment(
            document.AppointmentId,  // ✅ Cambiar de 'id' a 'AppointmentId'
            document.FacilityId,
            document.PatientId,
            document.MotherId,
            document.AppointmentDate,
            document.AppointmentTime,
            document.NurseId,
            AppointmentStatusExtensions.FromString(document.Status)
        );
    }
    public static object ToPersistence(Appointment appointment)
    {
        var data = appointment.ToPrimitives();

        return new
        {
            id = data.Id,
            facilityId = data.FacilityId,
            patientId = data.PatientId,
            motherId = data.MotherId,
            nurseId = data.NurseId,
            appointmentDate = data.AppointmentDate,
            appointmentTime = data.AppointmentTime,
            status = data.Status
        };
    }
}