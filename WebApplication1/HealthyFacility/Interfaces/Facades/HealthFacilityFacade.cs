using WebApplication1.HealthyFacility.Domain.Models.Aggregate;
using WebApplication1.HealthyFacility.Domain.Models.Commands;
using WebApplication1.HealthyFacility.Domain.Models.Entities;
using WebApplication1.HealthyFacility.Domain.Models.Queries;
using WebApplication1.HealthyFacility.Domain.Services;

namespace WebApplication1.HealthyFacility.Interfaces.Facades;



public class HealthFacilityFacade
{
    private readonly IHealthyFacilityCommandService _commandService;
    private readonly IHealthyFacilityQueryService _queryService;

    public HealthFacilityFacade(
        IHealthyFacilityCommandService commandService,
        IHealthyFacilityQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    // ==========================================
    // COMANDOS
    // ==========================================

    public async Task RegisterHealthFacilityAsync(RegisterHealthFacilityCommand command)
    {
        await _commandService.RegisterFacilityAsync(command);
    }

    public async Task AssignNurseToFacilityAsync(AssignNurseToFacilityCommand command)
    {
        await _commandService.AssignNurseToFacilityAsync(command);
    }

    public async Task BookAppointmentAsync(BookAppointmentCommand command)
    {
        await _commandService.BookAppointmentAsync(command);
    }

    public async Task CancelAppointmentAsync(CancelAppointmentCommand command)
    {
        await _commandService.CancelAppointmentAsync(command);
    }

    public async Task ValidatePatientBelongsToMotherAsync(string patientId, string motherId)
    {
        await _commandService.ValidatePatientBelongsToMotherAsync(patientId, motherId);
    }

    public async Task ValidateAppointmentBelongsToMotherAsync(string appointmentId, string motherId)
    {
        await _commandService.ValidateAppointmentBelongsToMotherAsync(appointmentId, motherId);
    }

    // ==========================================
    // QUERIES
    // ==========================================

    public async Task<List<object>> GetMyTopAppointmentsAsync(GetMyTopAppointmentsQuery query)
    {
        return await _queryService.GetMyTopAppointmentsAsync(query);
    }

    public async Task<MyAssignedFacilityResponse?> GetMyAssignedFacilityAsync(GetMyAssignedFacilityQuery query)
    {
        return await _queryService.GetMyAssignedFacilityAsync(query);
    }

    public async Task<object> ListHealthFacilitiesAsync(ListHealthFacilitiesQuery query)
    {
        return await _queryService.ListHealthFacilitiesAsync(query);
    }

    public async Task<HealthFacility?> GetHealthFacilityDetailAsync(GetHealthFacilityDetailQuery query)
    {
        return await _queryService.GetHealthFacilityDetailAsync(query);
    }

    public async Task<List<Appointment>> GetPatientAppointmentHistoryAsync(GetPatientAppointmentHistoryQuery query)
    {
        return await _queryService.GetPatientAppointmentHistoryAsync(query);
    }

    public async Task<List<Appointment>> GetNurseAppointmentScheduleAsync(GetNurseAppointmentScheduleQuery query)
    {
        return await _queryService.GetNurseAppointmentScheduleAsync(query);
    }

    public async Task<List<object>> GetFacilityAvailableSlotsAsync(GetFacilityAvailableSlotsQuery query)
    {
        return await _queryService.GetFacilityAvailableSlotsAsync(query);
    }

    public async Task<object> GetMotherNextAppointmentAsync(GetMotherNextAppointmentQuery query)
    {
        return await _queryService.GetMotherNextAppointmentAsync(query);
    }

    public async Task<CanRegisterResponseDto> CanRegisterFacilityAsync()
    {
        return await _queryService.CanRegisterFacilityAsync(new CanRegisterFacilityQuery());
    }

    public async Task<HealthFacilityAdminListResponseDto> ListAllHealthFacilitiesAsync()
    {
        return await _queryService.ListAllHealthFacilitiesAsync(new ListAllHealthFacilitiesQuery());
    }

    public async Task<List<UnassignedNurseDto>> ListUnassignedNursesAsync(ListUnassignedNursesQuery query)
    {
        return await _queryService.ListUnassignedNursesAsync(query);
    }
}