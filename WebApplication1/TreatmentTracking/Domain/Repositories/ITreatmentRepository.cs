using WebApplication1.TreatmentTracking.Domain.Model.Aggregate;
using WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;

namespace WebApplication1.TreatmentTracking.Domain.Repositories;

public interface ITreatmentRepository
{
    Task SaveAsync(Treatment treatment);
    Task UpdateAsync(Treatment treatment);
    Task<Treatment?> FindByIdAsync(string treatmentId);
    Task<Treatment?> FindActiveByPatientIdAsync(string patientId);
    Task<List<Treatment>> FindByPatientIdAsync(string patientId);
    Task<List<Treatment>> FindByNurseIdAsync(string nurseId, TreatmentStatus? status = null);
    Task<List<Treatment>> FindByRiskLevelAsync(RiskLevel riskLevel, string? nurseId = null);
    Task<List<Treatment>> FindAllActiveAsync();
    Task DeleteAsync(string treatmentId);
}