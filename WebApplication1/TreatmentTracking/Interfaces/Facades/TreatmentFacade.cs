using WebApplication1.patient_management.Domain.Repositories;
using WebApplication1.TreatmentTracking.Domain.Model.Commands;
using WebApplication1.TreatmentTracking.Domain.Model.Queries;
using WebApplication1.TreatmentTracking.Domain.Services;

namespace WebApplication1.TreatmentTracking.Interfaces.Facades;

public class TreatmentFacade
{
    private readonly ITreatmentCommandService _commandService;
    private readonly ITreatmentQueryService _queryService;
    private readonly IPatientRepository _patientRepository;

    public TreatmentFacade(
        ITreatmentCommandService commandService,
        ITreatmentQueryService queryService,
        IPatientRepository patientRepository)
    {
        _commandService = commandService;
        _queryService = queryService;
        _patientRepository = patientRepository;
    }

    // ==========================================
    // COMANDOS
    // ==========================================

    public async Task<object> StartTreatmentAsync(object command)
    {
        return await _commandService.StartTreatmentAsync((StartTreatmentCommand)command);
    }

    public async Task<object> ConfirmDoseAsync(object command)
    {
        return await _commandService.ConfirmDoseAsync((ConfirmDoseCommand)command);
    }

    public async Task<object> CompleteTreatmentAsync(object command)
    {
        return await _commandService.CompleteTreatmentAsync((CompleteTreatmentCommand)command);
    }

    public async Task<object> AbandonTreatmentAsync(object command)
    {
        return await _commandService.AbandonTreatmentAsync((AbandonTreatmentCommand)command);
    }

    public async Task<object> EvaluateMissedDoseAsync(object command)
    {
        return await _commandService.EvaluateMissedDoseAsync((EvaluateMissedDoseCommand)command);
    }

    // ==========================================
    // QUERIES
    // ==========================================

    public async Task<object> GetTodayDoseAsync(object query)
    {
        return await _queryService.GetTodayDoseAsync((GetTodayDoseQuery)query);
    }

    public async Task<object> GetPatientDoseHistoryAsync(object query)
    {
        var q = (GetPatientDoseHistoryQuery)query;
        // Validar que la madre tiene acceso al paciente (si viene motherId)
        // Nota: motherId se valida en el controlador
        return await _queryService.GetPatientDoseHistoryAsync(q);
    }

    public async Task<object> GetPendingPatientsByNurseAsync(object query)
    {
        return await _queryService.GetPendingPatientsByNurseAsync((GetPendingPatientsByNurseQuery)query);
    }

    public async Task<object> GetRiskLevelOverviewAsync(object query)
    {
        return await _queryService.GetRiskLevelOverviewAsync((GetRiskLevelOverviewQuery)query);
    }

    public async Task<object> GetTreatmentsByNurseAsync(object query)
    {
        return await _queryService.GetTreatmentsByNurseAsync((GetTreatmentsByNurseQuery)query);
    }

    public async Task<object> GetTreatmentDetailsAsync(object query)
    {
        return await _queryService.GetTreatmentDetailsAsync((GetTreatmentDetailsQuery)query);
    }

    public async Task<object> GetPatientsByRiskLevelAsync(object query)
    {
        return await _queryService.GetPatientsByRiskLevelAsync((GetPatientsByRiskLevelQuery)query);
    }

    public async Task<object> GetPatientTreatmentDetailAsync(object query)
    {
        return await _queryService.GetPatientTreatmentDetailAsync((GetPatientTreatmentDetailQuery)query);
    }

    // ==========================================
    // VALIDACIONES
    // ==========================================

    public async Task ValidateNurseHasPatientAsync(string nurseId, string patientId)
    {
        var patient = await _patientRepository.FindByIdAsync(patientId);
        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        var patientData = patient.ToPrimitives();
        if (patientData.NurseId != nurseId)
        {
            throw new Exception("Access denied: This patient is not assigned to you");
        }
    }

    public async Task ValidateMotherHasPatientAsync(string motherId, string patientId)
    {
        var patient = await _patientRepository.FindByIdAsync(patientId);
        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        var patientData = patient.ToPrimitives();
        if (patientData.MotherId != motherId)
        {
            throw new Exception("Access denied: This patient is not assigned to you");
        }
    }

    // ==========================================
    // MÉTODOS DE PRUEBA
    // ==========================================

    public async Task<object> ForceOmitDoseForTestingAsync(string dailyDoseId)
    {
        return await _commandService.ForceOmitDoseForTestingAsync(dailyDoseId);
    }

    public async Task<object> ForceConfirmDoseForTestingAsync(string dailyDoseId)
    {
        return await _commandService.ForceConfirmDoseForTestingAsync(dailyDoseId);
    }
}