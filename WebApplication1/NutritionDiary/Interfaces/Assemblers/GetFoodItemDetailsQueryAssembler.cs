using WebApplication1.NutritionDiary.Domain.Models.Queries;

namespace WebApplication1.NutritionDiary.Interfaces.Assemblers;

public static class GetFoodItemDetailsQueryAssembler
{
    public static GetFoodItemDetailsQuery ToQuery(string foodItemId)
    {
        return new GetFoodItemDetailsQuery(foodItemId);
    }
}