using WebApplication1.NutritionDiary.Domain.Models.Queries;

namespace WebApplication1.NutritionDiary.Interfaces.Assemblers;

public static class GetFoodItemsByCategoryQueryAssembler
{
    public static GetFoodItemsByCategoryQuery ToQuery(string category)
    {
        return new GetFoodItemsByCategoryQuery(category);
    }
}