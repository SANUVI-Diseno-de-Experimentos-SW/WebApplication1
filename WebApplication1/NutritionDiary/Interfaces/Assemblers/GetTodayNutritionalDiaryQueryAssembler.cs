using WebApplication1.NutritionDiary.Domain.Models.Queries;

namespace WebApplication1.NutritionDiary.Interfaces.Assemblers;

public static class GetTodayNutritionalDiaryQueryAssembler
{
    /// <summary>
    /// Acepta fecha opcional
    /// </summary>
    public static GetTodayNutritionalDiaryQuery ToQuery(string patientId, string? date = null)
    {
        return new GetTodayNutritionalDiaryQuery(patientId, date);
    }
}