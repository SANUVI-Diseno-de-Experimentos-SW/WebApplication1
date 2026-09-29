namespace WebApplication1.NutritionDiary.Domain.Models.Queries;

public record GetTodayNutritionalDiaryQuery(
    string PatientId,
    string? Date = null  // Formato: yyyy-MM-dd
);