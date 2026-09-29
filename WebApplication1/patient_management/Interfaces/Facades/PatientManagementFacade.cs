using WebApplication1.Contexts.PatientManagement.Domain.Commands;
using WebApplication1.patient_management.Domain;
using WebApplication1.patient_management.Domain.Aggregate;
using WebApplication1.patient_management.Domain.Commands;
using WebApplication1.patient_management.Domain.Entities;
using WebApplication1.patient_management.Domain.Model.DTos;
using WebApplication1.patient_management.Domain.Queries;
using WebApplication1.patient_management.Domain.Services;

namespace WebApplication1.patient_management.Interfaces.Facades;



public class PatientManagementFacade
{
    private readonly IPatientCommandService _commandService;
    private readonly IPatientQueryService _queryService;

    public PatientManagementFacade(
        IPatientCommandService commandService,
        IPatientQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    // ==========================================
    // COMANDOS
    // ==========================================

    public async Task RegisterPatientAsync(RegisterPatientCommand command)
    {
        await _commandService.RegisterPatientAsync(command);
    }

    public async Task AssignPatientToNurseAsync(AssignPatientToNurseCommand command)
    {
        await _commandService.AssignPatientToNurseAsync(command);
    }

    public async Task CreateInitialMedicalRecordAsync(CreateInitialMedicalRecordCommand command)
    {
        await _commandService.CreateInitialMedicalRecordAsync(command);
    }

    public async Task RegisterHemoglobinControlAsync(RegisterHemoglobinControlCommand command)
    {
        await _commandService.RegisterHemoglobinControlAsync(command);
    }

    public async Task DischargePatientAsync(DischargePatientCommand command)
    {
        await _commandService.DischargePatientAsync(command);
    }

    public async Task UpdateMedicalRecordAsync(UpdateMedicalRecordCommand command)
    {
        await _commandService.UpdateMedicalRecordAsync(command);
    }

    // ==========================================
    // QUERIES
    // ==========================================

    public async Task<object> SearchMotherByDniAsync(SearchMotherByDniQuery query)
    {
        return await _queryService.SearchMotherByDniAsync(query);
    }

    public async Task<IReadOnlyList<Patient>> ListPatientsByMotherAsync(ListPatientsByMotherQuery query)
    {
        return await _queryService.ListPatientsByMotherAsync(query);
    }

    public async Task<MedicalRecordWithPatientDto?> GetMedicalRecordAsync(GetMedicalRecordQuery query)
    {
        return await _queryService.GetMedicalRecordAsync(query);
    }
    public async Task<object> GetHemoglobinControlsHistoryAsync(GetHemoglobinControlsHistoryQuery query)
    {
        return await _queryService.GetHemoglobinControlsHistoryAsync(query);
    }

    public async Task<byte[]> DownloadMedicalRecordPdfAsync(DownloadMedicalRecordPdfQuery query)
    {
        return await _queryService.DownloadMedicalRecordPdfAsync(query);
    }

    public async Task<byte[]> DownloadHemoglobinReportPdfAsync(DownloadHemoglobinReportPdfQuery query)
    {
        return await _queryService.DownloadHemoglobinReportPdfAsync(query);
    }

    public async Task<IReadOnlyList<Patient>> GetPatientsEligibleForDischargeAsync(GetPatientsEligibleForDischargeQuery query)
    {
        return await _queryService.GetPatientsEligibleForDischargeAsync(query);
    }

    public async Task<IReadOnlyList<Patient>> GetPatientsAssignedToNurseAsync(GetPatientsAssignedToNurseQuery query)
    {
        return await _queryService.GetPatientsAssignedToNurseAsync(query);
    }

    public async Task<object> GetHemoglobinEvolutionChartAsync(GetHemoglobinEvolutionChartQuery query)
    {
        return await _queryService.GetHemoglobinEvolutionChartAsync(query);
    }

    public async Task<int> GetActivePatientsCountAsync(GetActivePatientsCountQuery query)
    {
        return await _queryService.GetActivePatientsCountAsync(query);
    }

    public async Task<object> GetMotherPatientsSummaryAsync(GetMotherPatientsSummaryQuery query)
    {
        return await _queryService.GetMotherPatientsSummaryAsync(query);
    }

    public async Task<Patient?> GetPatientBasicInfoAsync(GetPatientBasicInfoQuery query)
    {
        return await _queryService.GetPatientBasicInfoAsync(query);
    }

    public async Task<bool> CheckPatientMedicalRecordAsync(string patientId)
    {
        var query = new CheckPatientMedicalRecordQuery(patientId);
        return await _queryService.CheckPatientMedicalRecordAsync(query);
    }

    // ==========================================
    // VALIDACIONES
    // ==========================================

    public async Task ValidatePatientBelongsToMotherAsync(string patientId, string motherId)
    {
        var query = new GetPatientQuery(patientId);
        var patient = await _queryService.GetPatientAsync(query);

        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        var patientData = patient.ToPrimitives();

        if (patientData.MotherId != motherId)
        {
            throw new Exception("Access denied: This patient does not belong to you");
        }
    }

    public async Task ValidateNurseHasPatientAsync(string nurseId, string patientId)
    {
        var query = new GetPatientQuery(patientId);
        var patient = await _queryService.GetPatientAsync(query);

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

    public async Task ValidateNurseHasAccessToMedicalRecordAsync(string nurseId, string medicalRecordId)
    {
        var medicalRecord = await _queryService.GetMedicalRecordByIdAsync(medicalRecordId);

        if (medicalRecord == null)
        {
            throw new Exception("Medical record not found");
        }

        var medicalData = medicalRecord.ToPrimitives();

        var query = new GetPatientQuery(medicalData.PatientId);
        var patient = await _queryService.GetPatientAsync(query);

        if (patient == null)
        {
            throw new Exception("Patient not found");
        }

        var patientData = patient.ToPrimitives();

        if (patientData.NurseId != nurseId)
        {
            throw new Exception("Access denied: This medical record does not belong to a patient assigned to you");
        }
    }

    public async Task<Patient?> GetPatientAsync(string patientId)
    {
        var query = new GetPatientQuery(patientId);
        return await _queryService.GetPatientAsync(query);
    }
}