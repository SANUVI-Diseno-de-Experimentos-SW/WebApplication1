using WebApplication1.NutritionDiary.Domain.Models.Entities;
using WebApplication1.NutritionDiary.Infrastructure.Persitencia.Models;

namespace WebApplication1.NutritionDiary.Infrastructure.Mappers;

public static class FoodEntryMapper
{
    public static FoodEntry ToDomain(FoodEntryDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        return new FoodEntry(
            document.FoodEntryId,  // ✅ Usar FoodEntryId
            document.DiaryId,
            document.FoodItemId,
            document.Quantity,
            document.Unit,
            document.IronContributed,
            document.RegisteredAt
        );
    }

    public static FoodEntryDocument ToPersistence(FoodEntry entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        var data = entry.ToPrimitives();

        return new FoodEntryDocument
        {
            FoodEntryId = data.Id,
            DiaryId = data.DiaryId,
            FoodItemId = data.FoodItemId,
            Quantity = data.Quantity,
            Unit = data.Unit,
            IronContributed = data.IronContributed,
            RegisteredAt = data.RegisteredAt
        };
    }

    public static List<FoodEntry> ToDomainList(IEnumerable<FoodEntryDocument> documents)
    {
        return documents.Select(ToDomain).ToList();
    }
}