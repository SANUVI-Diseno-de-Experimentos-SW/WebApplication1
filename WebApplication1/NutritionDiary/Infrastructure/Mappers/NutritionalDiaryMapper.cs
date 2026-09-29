using WebApplication1.NutritionDiary.Domain.Models.Aggregate;
using WebApplication1.NutritionDiary.Infrastructure.Persitencia.Models;

namespace WebApplication1.NutritionDiary.Infrastructure.Mappers;

public static class NutritionalDiaryMapper
{
    public static NutritionalDiary ToDomain(NutritionalDiaryDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        return new NutritionalDiary(
            document.NutritionalDiaryId,  // ✅ Usar NutritionalDiaryId
            document.PatientId,
            document.MotherId,
            document.Date,
            document.TotalIronAbsorbed,
            document.HasInhibitor
        );
    }

    public static NutritionalDiaryDocument ToPersistence(NutritionalDiary diary)
    {
        if (diary == null)
            throw new ArgumentNullException(nameof(diary));

        var data = diary.ToPrimitives();

        return new NutritionalDiaryDocument
        {
            NutritionalDiaryId = data.Id,
            PatientId = data.PatientId,
            MotherId = data.MotherId,
            Date = data.Date,
            TotalIronAbsorbed = data.TotalIronAbsorbed,
            HasInhibitor = data.HasInhibitor
        };
    }

    public static List<NutritionalDiary> ToDomainList(IEnumerable<NutritionalDiaryDocument> documents)
    {
        return documents.Select(ToDomain).ToList();
    }
}