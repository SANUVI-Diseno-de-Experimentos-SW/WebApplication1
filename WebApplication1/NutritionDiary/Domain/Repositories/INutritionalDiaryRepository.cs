using WebApplication1.NutritionDiary.Domain.Models.Aggregate;

namespace WebApplication1.NutritionDiary.Domain.Repositories;

public interface INutritionalDiaryRepository
{
    Task SaveAsync(NutritionalDiary diary);
    Task UpdateAsync(NutritionalDiary diary);
    Task<NutritionalDiary?> FindTodayByPatientIdAsync(string patientId);
    Task<List<NutritionalDiary>> FindByPatientAndDateRangeAsync(string patientId, DateTime startDate, DateTime endDate);
}