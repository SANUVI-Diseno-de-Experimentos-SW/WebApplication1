namespace WebApplication1.NutritionDiary.Domain.Models.ValueObjects;

public class NutrientContent
{
    public double IronMg { get; }
    public string IronType { get; }

    public NutrientContent(double ironMg, string ironType)
    {
        if (ironMg < 0)
            throw new ArgumentException("Iron mg cannot be negative", nameof(ironMg));

        var validTypes = new[] { "hemo", "no-hemo" };
        if (!validTypes.Contains(ironType))
            throw new ArgumentException("Invalid iron type", nameof(ironType));

        IronMg = ironMg;
        IronType = ironType;
    }

    // Constructor privado para serialización
    private NutrientContent() { }

    public bool IsHemo() => IronType == "hemo";
    public bool IsNonHemo() => IronType == "no-hemo";

    public NutrientContentPrimitives ToPrimitives()
    {
        return new NutrientContentPrimitives
        {
            IronMg = IronMg,
            IronType = IronType
        };
    }

    public class NutrientContentPrimitives
    {
        public double IronMg { get; set; }
        public string IronType { get; set; } = string.Empty;
    }
}