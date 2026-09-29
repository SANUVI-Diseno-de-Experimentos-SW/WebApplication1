using WebApplication1.NutritionDiary.Domain.Models.Commands;
using WebApplication1.NutritionDiary.Interfaces.Resources;

namespace WebApplication1.NutritionDiary.Interfaces.Assemblers;

public static class RegisterFoodEntryCommandFromResourceAssembler
{
    public static RegisterFoodEntryCommand ToCommand(RegisterFoodEntryResource resource)
    {
        return new RegisterFoodEntryCommand(
            resource.PatientId,
            resource.MotherId,
            resource.FoodItemId,
            resource.Quantity
        );
    }
}