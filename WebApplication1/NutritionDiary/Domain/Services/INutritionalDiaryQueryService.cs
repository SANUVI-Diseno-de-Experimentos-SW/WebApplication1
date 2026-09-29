using WebApplication1.NutritionDiary.Domain.Models.Queries;

namespace WebApplication1.NutritionDiary.Domain.Services;

public interface INutritionalDiaryQueryService
{
    Task<object> GetTodayNutritionalDiaryAsync(GetTodayNutritionalDiaryQuery query);
    Task<object> GetFoodItemsByCategoryAsync(GetFoodItemsByCategoryQuery query);
    Task<object> SearchFoodItemsAsync(SearchFoodItemsQuery query);
    Task<object> GetFoodItemDetailsAsync(GetFoodItemDetailsQuery query);
    Task<object> GetNutritionalHistoryAsync(GetNutritionalHistoryQuery query);
}