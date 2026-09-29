using WebApplication1.HealthyFacility.Domain.Models.ValueObjects;

namespace WebApplication1.HealthyFacility.Domain.Models.Entities;



public class Appointment
{
    public string Id { get; }
    public string FacilityId { get; }
    public string PatientId { get; }
    public string MotherId { get; }
    public string? NurseId { get; private set; }
    public string AppointmentDate { get; }
    public string AppointmentTime { get; }
    public AppointmentStatus Status { get; private set; }

    public Appointment(
        string id,
        string facilityId,
        string patientId,
        string motherId,
        string appointmentDate,
        string appointmentTime,
        string? nurseId = null,
        AppointmentStatus status = AppointmentStatus.CONFIRMED)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Appointment id is required", nameof(id));

        if (string.IsNullOrWhiteSpace(facilityId))
            throw new ArgumentException("Facility id is required", nameof(facilityId));

        if (string.IsNullOrWhiteSpace(patientId))
            throw new ArgumentException("Patient id is required", nameof(patientId));

        if (string.IsNullOrWhiteSpace(motherId))
            throw new ArgumentException("Mother id is required", nameof(motherId));

        if (string.IsNullOrWhiteSpace(appointmentDate))
            throw new ArgumentException("Appointment date is required", nameof(appointmentDate));

        if (string.IsNullOrWhiteSpace(appointmentTime))
            throw new ArgumentException("Appointment time is required", nameof(appointmentTime));

        Id = id;
        FacilityId = facilityId;
        PatientId = patientId;
        MotherId = motherId;
        NurseId = nurseId;
        AppointmentDate = appointmentDate;
        AppointmentTime = appointmentTime;
        Status = status;
    }

    // Constructor privado para serialización
    private Appointment() { }

    public void AssignNurse(string nurseId)
    {
        if (string.IsNullOrWhiteSpace(nurseId))
            throw new ArgumentException("Nurse id is required", nameof(nurseId));

        NurseId = nurseId;
    }

    public void Cancel()
    {
        if (Status == AppointmentStatus.CANCELLED)
            throw new InvalidOperationException("Appointment already cancelled");

        Status = AppointmentStatus.CANCELLED;
    }

    public void Confirm()
    {
        Status = AppointmentStatus.CONFIRMED;
    }

    public AppointmentPrimitives ToPrimitives()
    {
        return new AppointmentPrimitives
        {
            Id = Id,
            FacilityId = FacilityId,
            PatientId = PatientId,
            MotherId = MotherId,
            NurseId = NurseId,
            AppointmentDate = AppointmentDate,
            AppointmentTime = AppointmentTime,
            Status = Status.ToStringValue()
        };
    }

    public class AppointmentPrimitives
    {
        public string Id { get; set; } = string.Empty;
        public string FacilityId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string MotherId { get; set; } = string.Empty;
        public string? NurseId { get; set; }
        public string AppointmentDate { get; set; } = string.Empty;
        public string AppointmentTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}