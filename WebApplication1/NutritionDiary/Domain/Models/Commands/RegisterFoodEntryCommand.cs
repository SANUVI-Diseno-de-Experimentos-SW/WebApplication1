namespace WebApplication1.NutritionDiary.Domain.Models.Commands;

public record RegisterFoodEntryCommand(
    string PatientId,
    string MotherId,
    string FoodItemId,
    double Quantity
);