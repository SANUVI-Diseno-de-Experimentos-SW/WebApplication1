using WebApplication1.TreatmentTracking.Domain.Model.Entities;

namespace WebApplication1.TreatmentTracking.Domain.Repositories;

public interface IDailyDoseRepository
{
    Task SaveManyAsync(List<DailyDose> doses);
    Task SaveAsync(DailyDose dose);
    Task UpdateAsync(DailyDose dose);
    Task<DailyDose?> FindByIdAsync(string dailyDoseId);
    Task<List<DailyDose>> FindByTreatmentIdAsync(string treatmentId);
    Task DeleteAsync(string dailyDoseId);
    Task DeleteManyAsync(List<string> dailyDoseIds);
    Task<DailyDose?> FindTodayDoseAsync(string treatmentId);
    Task<List<DailyDose>> FindPendingOlderThanHoursAsync(int hours);
}