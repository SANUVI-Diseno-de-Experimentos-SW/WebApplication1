namespace WebApplication1.shared.Scripts.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class FoodItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("id")]
    public string FoodId { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("nutrientContent")]
    public NutrientContent NutrientContent { get; set; } = new();

    [BsonElement("isInhibitor")]
    public bool IsInhibitor { get; set; }

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;
}

public class NutrientContent
{
    [BsonElement("ironMg")]
    public double IronMg { get; set; }

    [BsonElement("ironType")]
    public string IronType { get; set; } = string.Empty; // "hemo" o "no-hemo"
}