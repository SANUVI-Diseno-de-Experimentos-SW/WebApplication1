namespace WebApplication1.NutritionDiary.Interfaces.Resources;

public class RegisterFoodEntryResource
{
    public string PatientId { get; set; } = string.Empty;
    public string MotherId { get; set; } = string.Empty;
    public string FoodItemId { get; set; } = string.Empty;
    public double Quantity { get; set; }
}
