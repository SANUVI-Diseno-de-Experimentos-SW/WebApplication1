namespace WebApplication1.NutritionDiary.Domain.Services;

public interface IIronCalculatorService
{
    double CalculateIronAbsorption(double ironMg, double quantity, string ironType);
}