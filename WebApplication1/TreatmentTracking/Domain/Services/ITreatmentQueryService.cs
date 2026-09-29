using WebApplication1.TreatmentTracking.Domain.Model.Queries;

namespace WebApplication1.TreatmentTracking.Domain.Services;

public interface ITreatmentQueryService
{
    Task<object> GetTodayDoseAsync(GetTodayDoseQuery query);
    Task<object> GetPatientDoseHistoryAsync(GetPatientDoseHistoryQuery query);
    Task<object> GetPendingPatientsByNurseAsync(GetPendingPatientsByNurseQuery query);
    Task<object> GetRiskLevelOverviewAsync(GetRiskLevelOverviewQuery query);
    Task<object> GetTreatmentsByNurseAsync(GetTreatmentsByNurseQuery query);
    Task<object> GetTreatmentDetailsAsync(GetTreatmentDetailsQuery query);
    Task<object> GetPatientsByRiskLevelAsync(GetPatientsByRiskLevelQuery query);
    Task<object> GetPatientTreatmentDetailAsync(GetPatientTreatmentDetailQuery query);
}