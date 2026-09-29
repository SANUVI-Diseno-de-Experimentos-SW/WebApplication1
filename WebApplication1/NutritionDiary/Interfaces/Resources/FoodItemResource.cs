namespace WebApplication1.NutritionDiary.Interfaces.Resources;

public class FoodItemResource
{
    public string FoodItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IronType { get; set; } = string.Empty;
    public double IronMgPer100g { get; set; }
    public bool IsInhibitor { get; set; }
}