namespace WebApplication1.NutritionDiary.Domain.Models.Queries;

public record GetNutritionalHistoryQuery(
    string PatientId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);