using WebApplication1.NutritionDiary.Domain.Models.Queries;

namespace WebApplication1.NutritionDiary.Interfaces.Assemblers;

public static class SearchFoodItemsQueryAssembler
{
    public static SearchFoodItemsQuery ToQuery(string searchText)
    {
        return new SearchFoodItemsQuery(searchText);
    }
}