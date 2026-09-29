using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.HealthyFacility.Domain.Models.Aggregate;
using WebApplication1.HealthyFacility.Domain.Models.Commands;
using WebApplication1.HealthyFacility.Domain.Models.Entities;
using WebApplication1.HealthyFacility.Domain.Models.ValueObjects;
using WebApplication1.HealthyFacility.Domain.Repositories;
using WebApplication1.HealthyFacility.Domain.Services;
using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.shared.catalogs.Data;

namespace WebApplication1.HealthyFacility.Application.Services;



public class HealthFacilityCommandServiceImpl : IHealthyFacilityCommandService
{
    private readonly IHealthFacilityRepository _healthFacilityRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly INurseAssignmentRepository _nurseAssignmentRepository;
    private readonly DistrictRepository _districtRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPatientRepository _patientRepository;

    public HealthFacilityCommandServiceImpl(
        IHealthFacilityRepository healthFacilityRepository,
        IAppointmentRepository appointmentRepository,
        INurseAssignmentRepository nurseAssignmentRepository,
        DistrictRepository districtRepository,
        IUserRepository userRepository,
        IPatientRepository patientRepository)
    {
        _healthFacilityRepository = healthFacilityRepository;
        _appointmentRepository = appointmentRepository;
        _nurseAssignmentRepository = nurseAssignmentRepository;
        _districtRepository = districtRepository;
        _userRepository = userRepository;
        _patientRepository = patientRepository;
    }

    public async Task AssignNurseToFacilityAsync(AssignNurseToFacilityCommand command)
    {
        var facility = await _healthFacilityRepository.FindByIdAsync(command.FacilityId);
        if (facility == null)
        {
            throw new Exception("Facility not found");
        }

        var userNurse = await _userRepository.FindNurseByIdAsync(command.NurseId);
        if (userNurse == null)
        {
            throw new Exception("Nurse not found");
        }

        // Validar si la posta ya tiene enfermero
        var existingAssignment = await _nurseAssignmentRepository.FindActiveByFacilityIdAsync(command.FacilityId);
        if (existingAssignment != null)
        {
            throw new Exception($"Facility already has an assigned nurse. Current nurse ID: {existingAssignment.NurseId}");
        }

        var nurseExistingAssignment = await _nurseAssignmentRepository.FindActiveByNurseIdAsync(command.NurseId);
        if (nurseExistingAssignment != null)
        {
            throw new Exception($"Nurse is already assigned to another facility. Facility ID: {nurseExistingAssignment.FacilityId}");
        }

        var userNurseData = userNurse.ToPrimitives();

        var assignment = new NurseAssignment(
            Guid.NewGuid().ToString(),
            command.FacilityId,
            userNurseData.Id
        );

        facility.AssignNurse(assignment);

        await _nurseAssignmentRepository.SaveAsync(assignment);
        await _healthFacilityRepository.UpdateAsync(facility);
    }

    public async Task BookAppointmentAsync(BookAppointmentCommand command)
    {
        // Verificar si ya existe una cita en ese horario
        var existingAppointment = await _appointmentRepository.FindByFacilityAndDateTimeAsync(
            command.FacilityId,
            command.AppointmentDate,
            command.AppointmentTime
        );

        if (existingAppointment != null)
        {
            throw new Exception("This schedule is already reserved");
        }

        var patient = await _patientRepository.FindByIdAsync(command.PatientId);
        if (patient == null)
        {
            throw new Exception("Patient Not Registered");
        }

        // Obtener el enfermero asignado a la posta
        var nurseAssignment = await _nurseAssignmentRepository.FindActiveByFacilityIdAsync(command.FacilityId);
        if (nurseAssignment == null)
        {
            throw new Exception("This facility has no assigned nurse. Cannot book appointment.");
        }

        var patientData = patient.ToPrimitives();
        var nurseId = nurseAssignment.NurseId;

        var appointment = new Appointment(
            Guid.NewGuid().ToString(),
            command.FacilityId,
            patientData.Id,
            patientData.MotherId,
            command.AppointmentDate,
            command.AppointmentTime,
            nurseId,
            AppointmentStatus.CONFIRMED
        );

        await _appointmentRepository.SaveAsync(appointment);
    }

    public async Task CancelAppointmentAsync(CancelAppointmentCommand command)
    {
        var appointment = await _appointmentRepository.FindByIdAsync(command.AppointmentId);
        if (appointment == null)
        {
            throw new Exception("Appointment not found");
        }

        appointment.Cancel();

        await _appointmentRepository.UpdateAsync(appointment);
    }

    public async Task RegisterFacilityAsync(RegisterHealthFacilityCommand command)
    {
        var scheduleOfOperation = BuildScheduleOfOperation(command.AvailableDays, command.AvailableSlots);

        var district = _districtRepository.FindById(command.DistrictId);
        if (district == null)
        {
            throw new Exception("District not found");
        }

        var facility = new HealthFacility(
            Guid.NewGuid().ToString(),
            command.Name,
            command.Address,
            command.DistrictId,
            district.Name,
            new Coordinates(command.Latitude, command.Longitude),
            command.PhoneNumber,
            command.Services,
            new OperatingSchedule(command.AvailableDays, command.AvailableSlots),
            scheduleOfOperation,
            FacilityStatus.ACTIVE,
            new List<NurseAssignment>()
        );

        await _healthFacilityRepository.SaveAsync(facility);
    }

    public async Task ValidatePatientBelongsToMotherAsync(string patientId, string motherId)
    {
        var patient = await _patientRepository.FindByIdAsync(patientId);
        if (patient == null)
        {
            throw new Exception("Paciente no encontrado");
        }

        var patientData = patient.ToPrimitives();
        if (patientData.MotherId != motherId)
        {
            throw new Exception("Este paciente no pertenece a esta madre");
        }
    }

    public async Task ValidateAppointmentBelongsToMotherAsync(string appointmentId, string motherId)
    {
        var appointment = await _appointmentRepository.FindByIdAsync(appointmentId);
        if (appointment == null)
        {
            throw new Exception("Cita no encontrada");
        }

        var appointmentData = appointment.ToPrimitives();
        if (appointmentData.MotherId != motherId)
        {
            throw new Exception("Esta cita no pertenece a esta madre");
        }
    }

    private string BuildScheduleOfOperation(List<string> availableDays, List<string> availableSlots)
    {
        var firstDay = availableDays.FirstOrDefault() ?? "N/A";
        var lastDay = availableDays.LastOrDefault() ?? "N/A";
        var firstSlot = availableSlots.FirstOrDefault() ?? "N/A";
        var lastSlot = availableSlots.LastOrDefault() ?? "N/A";

        return $"{firstDay} to {lastDay} from {firstSlot} to {lastSlot}";
    }
}