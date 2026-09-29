using WebApplication1.HealthyFacility.Domain.Models.Aggregate;
using WebApplication1.HealthyFacility.Domain.Models.Entities;
using WebApplication1.HealthyFacility.Domain.Models.Queries;

namespace WebApplication1.HealthyFacility.Domain.Services;

public interface IHealthyFacilityQueryService
{
    Task<object> ListHealthFacilitiesAsync(ListHealthFacilitiesQuery query);
    Task<HealthFacility?> GetHealthFacilityDetailAsync(GetHealthFacilityDetailQuery query);
    Task<List<Appointment>> GetPatientAppointmentHistoryAsync(GetPatientAppointmentHistoryQuery query);
    Task<List<Appointment>> GetNurseAppointmentScheduleAsync(GetNurseAppointmentScheduleQuery query);
    Task<List<object>> GetFacilityAvailableSlotsAsync(GetFacilityAvailableSlotsQuery query);
    Task<object> GetMotherNextAppointmentAsync(GetMotherNextAppointmentQuery query);
    Task<List<UnassignedNurseDto>> ListUnassignedNursesAsync(ListUnassignedNursesQuery query);
    Task<CanRegisterResponseDto> CanRegisterFacilityAsync(CanRegisterFacilityQuery query);
    Task<HealthFacilityAdminListResponseDto> ListAllHealthFacilitiesAsync(ListAllHealthFacilitiesQuery query);
    Task<List<object>> GetMyTopAppointmentsAsync(GetMyTopAppointmentsQuery query);
    Task<MyAssignedFacilityResponse?> GetMyAssignedFacilityAsync(GetMyAssignedFacilityQuery query);
}

// ==========================================
// DTOs
// ==========================================

public class UnassignedNurseDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class CanRegisterResponseDto
{
    public bool CanRegister { get; }
    public string Message { get; }
    public string? Details { get; }

    public CanRegisterResponseDto(bool canRegister, string message, string? details = null)
    {
        CanRegister = canRegister;
        Message = message;
        Details = details;
    }
}

public class HealthFacilityAdminItemDto
{
    public string Id { get; }
    public string Name { get; }
    public string Address { get; }
    public string? AssignedNurseName { get; }
    public bool HasNurseAssigned { get; }
    public string? DisplayMessage { get; }

    public HealthFacilityAdminItemDto(
        string id,
        string name,
        string address,
        string? assignedNurseName,
        bool hasNurseAssigned,
        string? displayMessage = null)
    {
        Id = id;
        Name = name;
        Address = address;
        AssignedNurseName = assignedNurseName;
        HasNurseAssigned = hasNurseAssigned;
        DisplayMessage = displayMessage;
    }
}
public class HealthFacilityAdminListResponseDto
{
    public int Total { get; }
    public List<HealthFacilityAdminItemDto> HealthFacilities { get; }

    public HealthFacilityAdminListResponseDto(int total, List<HealthFacilityAdminItemDto> healthFacilities)
    {
        Total = total;
        HealthFacilities = healthFacilities;
    }
}
// ✅ CORREGIDO - Usar tipos nullable o solo propiedades necesarias
public class MyAssignedFacilityResponse
{
    public HealthFacility? HealthFacility { get; set; }
    public NurseAssignment? NurseAssignment { get; set; }
}