using WebApplication1.NutritionDiary.Domain.Models.Entities;
using WebApplication1.NutritionDiary.Domain.Models.ValueObjects;
using WebApplication1.NutritionDiary.Infrastructure.Persitencia.Models;

namespace WebApplication1.NutritionDiary.Infrastructure.Mappers;

public static class FoodItemMapper
{
    public static FoodItem ToDomain(FoodItemDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        return new FoodItem(
            document.FoodItemId,  // ✅ Usar FoodItemId
            document.Name,
            new NutrientContent(
                document.NutrientContent.IronMg,
                document.NutrientContent.IronType
            ),
            document.IsInhibitor,
            FoodCategoryExtensions.FromString(document.Category)
        );
    }

    public static FoodItemDocument ToPersistence(FoodItem foodItem)
    {
        if (foodItem == null)
            throw new ArgumentNullException(nameof(foodItem));

        var data = foodItem.ToPrimitives();

        return new FoodItemDocument
        {
            FoodItemId = data.Id,
            Name = data.Name,
            NutrientContent = new NutrientContentDocument
            {
                IronMg = data.NutrientContent.IronMg,
                IronType = data.NutrientContent.IronType
            },
            IsInhibitor = data.IsInhibitor,
            Category = data.Category
        };
    }

    public static List<FoodItem> ToDomainList(IEnumerable<FoodItemDocument> documents)
    {
        return documents.Select(ToDomain).ToList();
    }
}