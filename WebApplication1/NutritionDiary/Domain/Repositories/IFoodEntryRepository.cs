using WebApplication1.NutritionDiary.Domain.Models.Entities;

namespace WebApplication1.NutritionDiary.Domain.Repositories;

public interface IFoodEntryRepository
{
    Task SaveAsync(FoodEntry entry);
    Task<List<FoodEntry>> FindByDiaryIdAsync(string diaryId);
    Task<int> CountByDiaryIdAsync(string diaryId);
}