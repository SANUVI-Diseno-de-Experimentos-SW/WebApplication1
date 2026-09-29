using WebApplication1.AchievementsRewards.Domain.Model.Aggregate;

namespace WebApplication1.AchievementsRewards.Domain.Repositories;

public interface IAchievementRepository
{
    Task SaveAsync(Achievement achievement);
    Task UpdateAsync(Achievement achievement);
    Task<Achievement?> FindByIdAsync(string id);
    Task DeleteAsync(string id);
    Task<Achievement?> FindByTreatmentIdAsync(string treatmentId);
    Task<Achievement?> FindByPatientIdAsync(string patientId);
    Task<List<Achievement>> FindByMotherIdAsync(string motherId);
    Task<List<Achievement>> FindAllActiveAsync();
}