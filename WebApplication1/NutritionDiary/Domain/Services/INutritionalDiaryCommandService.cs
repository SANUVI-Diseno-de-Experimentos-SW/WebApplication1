using WebApplication1.NutritionDiary.Domain.Models.Commands;

namespace WebApplication1.NutritionDiary.Domain.Services;

public interface INutritionalDiaryCommandService
{
    Task<object> RegisterFoodEntryAsync(RegisterFoodEntryCommand command);
    Task ValidatePatientBelongsToMotherAsync(string patientId, string motherId);
}