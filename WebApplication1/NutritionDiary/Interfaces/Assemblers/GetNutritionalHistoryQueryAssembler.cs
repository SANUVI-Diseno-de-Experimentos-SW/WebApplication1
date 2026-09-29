using WebApplication1.NutritionDiary.Domain.Models.Queries;

namespace WebApplication1.NutritionDiary.Interfaces.Assemblers;

public static class GetNutritionalHistoryQueryAssembler
{
    public static GetNutritionalHistoryQuery ToQuery(string patientId, DateTime? startDate = null, DateTime? endDate = null)
    {
        return new GetNutritionalHistoryQuery(patientId, startDate, endDate);
    }
}