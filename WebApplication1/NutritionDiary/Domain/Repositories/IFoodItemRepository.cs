using WebApplication1.NutritionDiary.Domain.Models.Entities;

namespace WebApplication1.NutritionDiary.Domain.Repositories;

public interface IFoodItemRepository
{
    Task<FoodItem?> FindByIdAsync(string foodItemId);
    Task<List<FoodItem>> FindByCategoryAsync(string category);
    Task<List<FoodItem>> SearchByNameAsync(string searchText);
}