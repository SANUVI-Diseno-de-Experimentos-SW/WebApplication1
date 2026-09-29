
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.NutritionDiary.Infrastructure.Persitencia.Models;
public class NutrientContentDocument
{
    public double IronMg { get; set; }
    public string IronType { get; set; } = string.Empty;
}

public class FoodItemDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string FoodItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public NutrientContentDocument NutrientContent { get; set; } = new();
    public bool IsInhibitor { get; set; }
    public string Category { get; set; } = string.Empty;
}