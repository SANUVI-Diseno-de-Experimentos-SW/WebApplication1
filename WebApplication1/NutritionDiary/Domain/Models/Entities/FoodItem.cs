using WebApplication1.NutritionDiary.Domain.Models.ValueObjects;

namespace WebApplication1.NutritionDiary.Domain.Models.Entities;


public class FoodItem
{
    public string Id { get; }
    public string Name { get; }
    public NutrientContent NutrientContent { get; }
    public bool IsInhibitor { get; }
    public FoodCategory Category { get; }

    public FoodItem(
        string id,
        string name,
        NutrientContent nutrientContent,
        bool isInhibitor,
        FoodCategory category)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Food item id is required", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Food name is required", nameof(name));

        Id = id;
        Name = name;
        NutrientContent = nutrientContent ?? throw new ArgumentNullException(nameof(nutrientContent));
        IsInhibitor = isInhibitor;
        Category = category;
    }

    // Constructor privado para serialización
    private FoodItem() { }

    public bool IsHemoIron() => NutrientContent.IsHemo();
    public bool IsNonHemoIron() => NutrientContent.IsNonHemo();

    public FoodItemPrimitives ToPrimitives()
    {
        return new FoodItemPrimitives
        {
            Id = Id,
            Name = Name,
            NutrientContent = NutrientContent.ToPrimitives(),
            IsInhibitor = IsInhibitor,
            Category = Category.ToStringValue()
        };
    }

    public class FoodItemPrimitives
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public NutrientContent.NutrientContentPrimitives NutrientContent { get; set; } = new();
        public bool IsInhibitor { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}