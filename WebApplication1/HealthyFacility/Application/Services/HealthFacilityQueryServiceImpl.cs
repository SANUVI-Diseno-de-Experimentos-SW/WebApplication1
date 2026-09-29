

using WebApplication1.Contexts.IAM.Domain.Repositories;
using WebApplication1.HealthyFacility.Domain.Models.Aggregate;
using WebApplication1.HealthyFacility.Domain.Models.Entities;
using WebApplication1.HealthyFacility.Domain.Models.Queries;
using WebApplication1.HealthyFacility.Domain.Repositories;
using WebApplication1.HealthyFacility.Domain.Services;
using WebApplication1.HealthyFacility.Infrastructure.ExternalServicies;
using WebApplication1.patient_management.Domain.Repositories;

// ✅ Usar alias para los DTOs de Domain.Services
using CanRegisterResponseDto = WebApplication1.HealthyFacility.Domain.Services.CanRegisterResponseDto;
using HealthFacilityAdminListResponseDto = WebApplication1.HealthyFacility.Domain.Services.HealthFacilityAdminListResponseDto;
using HealthFacilityAdminItemDto = WebApplication1.HealthyFacility.Domain.Services.HealthFacilityAdminItemDto;

namespace WebApplication1.HealthyFacility.Application.Services;

public class HealthFacilityQueryServiceImpl : IHealthyFacilityQueryService
{
    private readonly IHealthFacilityRepository _healthFacilityRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IUserRepository _userRepository;
    private readonly INurseAssignmentRepository _nurseAssignmentRepository;

    public HealthFacilityQueryServiceImpl(
        IHealthFacilityRepository healthFacilityRepository,
        IAppointmentRepository appointmentRepository,
        IPatientRepository patientRepository,
        IUserRepository userRepository,
        INurseAssignmentRepository nurseAssignmentRepository)
    {
        _healthFacilityRepository = healthFacilityRepository;
        _appointmentRepository = appointmentRepository;
        _patientRepository = patientRepository;
        _userRepository = userRepository;
        _nurseAssignmentRepository = nurseAssignmentRepository;
    }

    public async Task<List<object>> GetMyTopAppointmentsAsync(GetMyTopAppointmentsQuery query)
    {
        var limit = query.Limit ?? 4;
        var allAppointments = await _appointmentRepository.FindConfirmedByNurseIdAsync(query.NurseId);

        var now = DateTime.UtcNow;

        var futureAppointments = allAppointments
            .Where(a =>
            {
                var data = a.ToPrimitives();
                var appointmentDateTime = DateTime.Parse($"{data.AppointmentDate} {data.AppointmentTime}");
                return appointmentDateTime > now;
            })
            .OrderBy(a =>
            {
                var data = a.ToPrimitives();
                return DateTime.Parse($"{data.AppointmentDate} {data.AppointmentTime}");
            })
            .Take(limit)
            .ToList();

        var enriched = new List<object>();

        foreach (var appointment in futureAppointments)
        {
            var data = appointment.ToPrimitives();

            var patient = await _patientRepository.FindByIdAsync(data.PatientId);
            var patientName = patient != null
                ? $"{patient.ToPrimitives().Name} {patient.ToPrimitives().LastName}"
                : "Desconocido";

            enriched.Add(new
            {
                appointmentId = data.Id,
                patientId = data.PatientId,
                patientName,
                facilityId = data.FacilityId,
                appointmentDate = data.AppointmentDate,
                appointmentTime = data.AppointmentTime,
                status = data.Status
            });
        }

        return enriched;
    }

    public async Task<MyAssignedFacilityResponse?> GetMyAssignedFacilityAsync(GetMyAssignedFacilityQuery query)
    {
        var nurseAssignment = await _nurseAssignmentRepository.FindActiveByNurseIdAsync(query.NurseId);
        if (nurseAssignment == null)
        {
            return null;
        }

        var facility = await _healthFacilityRepository.FindByIdAsync(nurseAssignment.FacilityId);
        if (facility == null)
        {
            return null;
        }

        return new MyAssignedFacilityResponse
        {
            HealthFacility = facility,
            NurseAssignment = nurseAssignment
        };
    }

    // ✅ Usar el alias CanRegisterResponseDto (de Domain.Services)
    public async Task<CanRegisterResponseDto> CanRegisterFacilityAsync(CanRegisterFacilityQuery query)
    {
        var allNurses = await _userRepository.FindAllNursesAsync();

        if (allNurses == null || allNurses.Count == 0)
        {
            return new CanRegisterResponseDto(
                false,
                "Sin enfermeros disponibles",
                "No hay personal de enfermería registrado en el sistema."
            );
        }

        var unassignedCount = 0;

        foreach (var nurse in allNurses)
        {
            var nurseData = nurse.ToPrimitives();
            var activeAssignment = await _nurseAssignmentRepository.FindActiveByNurseIdAsync(nurseData.Id);
            if (activeAssignment == null) unassignedCount++;
        }

        if (unassignedCount > 0)
        {
            return new CanRegisterResponseDto(
                true,
                $"Hay {unassignedCount} enfermero{(unassignedCount != 1 ? "s" : "")} disponible{(unassignedCount != 1 ? "s" : "")} para asignar a una nueva posta"
            );
        }

        return new CanRegisterResponseDto(
            false,
            "Sin enfermeros disponibles",
            "Actualmente, todo el personal de enfermería registrado ha sido asignado a una posta médica. Por favor, espere al registro de nuevo personal."
        );
    }

    // ✅ Usar los alias HealthFacilityAdminListResponseDto y HealthFacilityAdminItemDto
    public async Task<HealthFacilityAdminListResponseDto> ListAllHealthFacilitiesAsync(ListAllHealthFacilitiesQuery query)
    {
        var allFacilities = await _healthFacilityRepository.FindAllAsync();

        var items = new List<HealthFacilityAdminItemDto>();

        foreach (var facility in allFacilities)
        {
            var data = facility.ToPrimitives();

            var activeAssignment = await _nurseAssignmentRepository.FindActiveByFacilityIdAsync(data.Id);

            if (activeAssignment == null)
            {
                items.Add(new HealthFacilityAdminItemDto(
                    data.Id,
                    data.Name,
                    data.Address,
                    null,
                    false,
                    "No nurse assigned yet"
                ));
                continue;
            }

            var nurse = await _userRepository.FindNurseByIdAsync(activeAssignment.NurseId);
            var nurseData = nurse?.ToPrimitives();

            items.Add(new HealthFacilityAdminItemDto(
                data.Id,
                data.Name,
                data.Address,
                nurseData != null ? $"{nurseData.Name} {nurseData.Lastname}" : null,
                true
            ));
        }

        return new HealthFacilityAdminListResponseDto(items.Count, items);
    }

    public async Task<List<UnassignedNurseDto>> ListUnassignedNursesAsync(ListUnassignedNursesQuery query)
    {
        var allNurses = await _userRepository.FindAllNursesAsync();

        if (allNurses == null || allNurses.Count == 0)
        {
            return new List<UnassignedNurseDto>();
        }

        var result = new List<UnassignedNurseDto>();

        foreach (var nurse in allNurses)
        {
            var nurseData = nurse.ToPrimitives();

            var activeAssignment = await _nurseAssignmentRepository.FindActiveByNurseIdAsync(nurseData.Id);

            if (activeAssignment == null)
            {
                result.Add(new UnassignedNurseDto
                {
                    Id = nurseData.Id,
                    FullName = $"{nurseData.Name} {nurseData.Lastname}"
                });
            }
        }

        return result;
    }

    public async Task<object> ListHealthFacilitiesAsync(ListHealthFacilitiesQuery query)
    {
        await ValidateMotherHasPatientsAsync(query.MotherId);

        var facilities = await _healthFacilityRepository.FindActiveFacilitiesAsync();

        var result = new List<object>();

        foreach (var facility in facilities)
        {
            var data = facility.ToPrimitives();

            var distanceKm = DistanceCalculatorService.CalculateDistanceKm(
                query.UserLatitude,
                query.UserLongitude,
                data.Coordinates.Lat,
                data.Coordinates.Lng
            );

            // ✅ CAMBIADO: Ahora devuelve un objeto plano con todos los campos
            result.Add(new
            {
                id = data.Id,
                name = data.Name,
                address = data.Address,
                latitude = data.Coordinates.Lat,
                longitude = data.Coordinates.Lng,
                phoneNumber = data.PhoneNumber,
                services = data.Services,
                availableDays = data.OperatingSchedule.AvailableDays,
                distanceKm = distanceKm,
                status = data.Status
            });
        }

        return result;
    }

    public async Task<HealthFacility?> GetHealthFacilityDetailAsync(GetHealthFacilityDetailQuery query)
    {
        return await _healthFacilityRepository.FindByIdAsync(query.FacilityId);
    }

    public async Task<List<Appointment>> GetPatientAppointmentHistoryAsync(GetPatientAppointmentHistoryQuery query)
    {
        return await _appointmentRepository.FindByPatientIdAsync(query.PatientId);
    }

    public async Task<List<Appointment>> GetNurseAppointmentScheduleAsync(GetNurseAppointmentScheduleQuery query)
    {
        var allAppointments = await _appointmentRepository.FindConfirmedByNurseIdAsync(query.NurseId);
    
        var now = DateTime.UtcNow;
        var today = now.ToString("yyyy-MM-dd");
        var currentTime = now.ToString("HH:mm");
    
        // Filtrar solo citas CONFIRMADAS y FUTURAS
        var futureAppointments = allAppointments
            .Where(a => 
            {
                var data = a.ToPrimitives();
                var dateComparison = string.Compare(data.AppointmentDate, today);
            
                if (dateComparison > 0) return true; // Fecha futura
            
                if (dateComparison == 0) // Misma fecha
                {
                    return string.Compare(data.AppointmentTime, currentTime) > 0; // Hora futura
                }
            
                return false; // Fecha pasada
            })
            .OrderBy(a => 
            {
                var data = a.ToPrimitives();
                return DateTime.Parse($"{data.AppointmentDate} {data.AppointmentTime}");
            })
            .ToList();
    
        return futureAppointments;
    }

    public async Task<List<object>> GetFacilityAvailableSlotsAsync(GetFacilityAvailableSlotsQuery query)
    {
        var facility = await _healthFacilityRepository.FindByIdAsync(query.FacilityId);
        if (facility == null)
        {
            throw new Exception("Health facility not found");
        }

        var facilityData = facility.ToPrimitives();

        var appointments = await _appointmentRepository.FindByFacilityAndDateAsync(
            query.FacilityId,
            query.AppointmentDate
        );

        var occupiedTimes = appointments.Select(a => a.ToPrimitives().AppointmentTime).ToHashSet();

        return facilityData.OperatingSchedule.AvailableSlots
            .Select(time => new
            {
                time,
                status = occupiedTimes.Contains(time) ? "OCCUPIED" : "AVAILABLE"
            })
            .Cast<object>()
            .ToList();
    }

    public async Task<object> GetMotherNextAppointmentAsync(GetMotherNextAppointmentQuery query)
    {
        var appointment = await _appointmentRepository.FindNextAppointmentByMotherIdAsync(query.MotherId);

        if (appointment == null)
        {
            return null!;
        }

        var appointmentData = appointment.ToPrimitives();

        var facility = await _healthFacilityRepository.FindByIdAsync(appointmentData.FacilityId);

        return new
        {
            appointment,
            facilityName = facility?.ToPrimitives().Name ?? "Unknown"
        };
    }

    private async Task ValidateMotherHasPatientsAsync(string motherId)
    {
        var patients = await _patientRepository.FindByMotherIdAsync(motherId);
        if (patients == null || patients.Count == 0)
        {
            throw new Exception("Debes registrar al menos un paciente antes de usar esta función");
        }
    }
}