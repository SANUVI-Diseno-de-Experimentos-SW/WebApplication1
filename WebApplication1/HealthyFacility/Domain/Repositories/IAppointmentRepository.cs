using WebApplication1.HealthyFacility.Domain.Models.Entities;

namespace WebApplication1.HealthyFacility.Domain.Repositories;


public interface IAppointmentRepository
{
    /// <summary>
    /// Saves the appointment to the repository.
    /// </summary>
    Task<Appointment> SaveAsync(Appointment appointment);

    /// <summary>
    /// Finds an appointment by its ID.
    /// </summary>
    Task<Appointment?> FindByIdAsync(string id);

    /// <summary>
    /// Finds appointments by PatientId
    /// </summary>
    Task<List<Appointment>> FindByPatientIdAsync(string patientId);

    /// <summary>
    /// Finds an appointment by facility id, appointment date and appointment time.
    /// It was introduced by the rule that a mother cannot book a date/time already
    /// taken by another mother at the same facility.
    /// </summary>
    Task<Appointment?> FindByFacilityAndDateTimeAsync(string facilityId, string appointmentDate, string appointmentTime);

    /// <summary>
    /// Updates the appointment in the repository.
    /// </summary>
    Task UpdateAsync(Appointment appointment);

    /// <summary>
    /// Finds appointments confirmed for a nurse by nurse id.
    /// </summary>
    Task<List<Appointment>> FindConfirmedByNurseIdAsync(string nurseId);

    /// <summary>
    /// Finds appointments by facility and date.
    /// </summary>
    Task<List<Appointment>> FindByFacilityAndDateAsync(string facilityId, string appointmentDate);

    /// <summary>
    /// Finds the next appointment by mother id.
    /// </summary>
    Task<Appointment?> FindNextAppointmentByMotherIdAsync(string motherId);
}