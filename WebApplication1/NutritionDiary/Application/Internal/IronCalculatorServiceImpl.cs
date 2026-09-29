using WebApplication1.NutritionDiary.Domain.Services;

namespace WebApplication1.NutritionDiary.Application.Internal;

    
public class IronCalculatorServiceImpl : IIronCalculatorService
{
    public double CalculateIronAbsorption(double ironMg, double quantity, string ironType)
    {
        if (ironMg < 0)
            throw new ArgumentException("Iron amount cannot be negative", nameof(ironMg));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        var totalIron = (ironMg / 100) * quantity;

        double absorbedIron = ironType switch
        {
            "hemo" => totalIron * 0.25,
            "no-hemo" => totalIron * 0.05,
            _ => throw new ArgumentException("Invalid iron type", nameof(ironType))
        };

        return Math.Round(absorbedIron, 2);
    }
}