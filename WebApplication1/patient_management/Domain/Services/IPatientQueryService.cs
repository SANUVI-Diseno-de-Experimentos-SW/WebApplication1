using WebApplication1.patient_management.Domain.Aggregate;
using WebApplication1.patient_management.Domain.Entities;
using WebApplication1.patient_management.Domain.Model.DTos;
using WebApplication1.patient_management.Domain.Queries;

namespace WebApplication1.patient_management.Domain.Services;


/// <summary>
/// Servicio de queries para pacientes
/// </summary>
public interface IPatientQueryService
{
    // ==========================================
    // PACIENTES
    // ==========================================

    Task<Patient?> GetPatientAsync(GetPatientQuery query);
    Task<Patient?> GetPatientBasicInfoAsync(GetPatientBasicInfoQuery query);
    Task<IReadOnlyList<Patient>> GetPatientsAssignedToNurseAsync(GetPatientsAssignedToNurseQuery query);
    Task<IReadOnlyList<Patient>> GetPatientsEligibleForDischargeAsync(GetPatientsEligibleForDischargeQuery query);
    Task<IReadOnlyList<Patient>> ListPatientsByMotherAsync(ListPatientsByMotherQuery query);
    Task<int> GetActivePatientsCountAsync(GetActivePatientsCountQuery query);
    Task<object> GetMotherPatientsSummaryAsync(GetMotherPatientsSummaryQuery query);
    Task<object?> SearchMotherByDniAsync(SearchMotherByDniQuery query);

    // ==========================================
    // HISTORIA CLÍNICA
    // ==========================================

    Task<MedicalRecordWithPatientDto?> GetMedicalRecordAsync(GetMedicalRecordQuery query);
    Task<bool> CheckPatientMedicalRecordAsync(CheckPatientMedicalRecordQuery query);

    // ==========================================
    // PDFS
    // ==========================================

    Task<byte[]> DownloadMedicalRecordPdfAsync(DownloadMedicalRecordPdfQuery query);
    Task<byte[]> DownloadHemoglobinReportPdfAsync(DownloadHemoglobinReportPdfQuery query);

    // ==========================================
    // CONTROLES Y GRÁFICOS
    // ==========================================

    Task<MedicalRecord?> GetMedicalRecordByIdAsync(string medicalRecordId);

    Task<object> GetHemoglobinControlsHistoryAsync(GetHemoglobinControlsHistoryQuery query);
    Task<object> GetHemoglobinEvolutionChartAsync(GetHemoglobinEvolutionChartQuery query);
        
}