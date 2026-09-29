using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.NutritionDiary.Infrastructure.Persitencia.Models;


public class FoodEntryDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string FoodEntryId { get; set; } = string.Empty;
    public string DiaryId { get; set; } = string.Empty;
    public string FoodItemId { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double IronContributed { get; set; }
    public DateTime RegisteredAt { get; set; }
}