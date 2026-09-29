namespace WebApplication1.NutritionDiary.Interfaces.Resources;

public class TodayNutritionalDiaryResource
{
    public string? DiaryId { get; set; }
    public DateTime Date { get; set; }
    public double TotalIronAbsorbed { get; set; }
    public List<object> FoodEntries { get; set; } = new();
}