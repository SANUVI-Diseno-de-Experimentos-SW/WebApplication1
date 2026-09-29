using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.NutritionDiary.Infrastructure.Persitencia.Models;

public class NutritionalDiaryDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string NutritionalDiaryId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string MotherId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double TotalIronAbsorbed { get; set; }
    public bool HasInhibitor { get; set; }
}