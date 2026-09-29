namespace WebApplication1.NutritionDiary.Interfaces.Resources;


public class NutritionalHistoryResource
{
    public string PatientId { get; set; } = string.Empty;
    public Period Period { get; set; } = new();
    public List<object> Days { get; set; } = new();
}

public class Period
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}