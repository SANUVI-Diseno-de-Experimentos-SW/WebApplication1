namespace WebApplication1.NutritionDiary.Domain.Models.ValueObjects;
public enum FoodCategory
{
    MEAT,
    FISH,
    VEGETABLE,
    LEGUME,
    DAIRY,
    GRAIN,
    FRUIT,
    BEVERAGE
}

public static class FoodCategoryExtensions
{
    public static string ToStringValue(this FoodCategory category)
    {
        return category switch
        {
            FoodCategory.MEAT => "MEAT",
            FoodCategory.FISH => "FISH",
            FoodCategory.VEGETABLE => "VEGETABLE",
            FoodCategory.LEGUME => "LEGUME",
            FoodCategory.DAIRY => "DAIRY",
            FoodCategory.GRAIN => "GRAIN",
            FoodCategory.FRUIT => "FRUIT",
            FoodCategory.BEVERAGE => "BEVERAGE",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };
    }

    public static FoodCategory FromString(string value)
    {
        return value switch
        {
            "MEAT" => FoodCategory.MEAT,
            "FISH" => FoodCategory.FISH,
            "VEGETABLE" => FoodCategory.VEGETABLE,
            "LEGUME" => FoodCategory.LEGUME,
            "DAIRY" => FoodCategory.DAIRY,
            "GRAIN" => FoodCategory.GRAIN,
            "FRUIT" => FoodCategory.FRUIT,
            "BEVERAGE" => FoodCategory.BEVERAGE,
            _ => throw new ArgumentException($"Invalid food category: {value}")
        };
    }

    public static string GetDisplayName(this FoodCategory category)
    {
        return category switch
        {
            FoodCategory.MEAT => "Carnes",
            FoodCategory.FISH => "Pescados",
            FoodCategory.VEGETABLE => "Verduras",
            FoodCategory.LEGUME => "Legumbres",
            FoodCategory.DAIRY => "Lácteos",
            FoodCategory.GRAIN => "Cereales",
            FoodCategory.FRUIT => "Frutas",
            FoodCategory.BEVERAGE => "Bebidas",
            _ => category.ToString()
        };
    }

    public static bool IsInhibitorCategory(this FoodCategory category)
    {
        return category == FoodCategory.DAIRY || category == FoodCategory.BEVERAGE;
    }
}