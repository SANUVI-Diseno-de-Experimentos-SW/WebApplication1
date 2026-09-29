using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace WebApplication1.TreatmentTracking.Infrastructure.Persitencia.MongoDb.Models;

public class DailyDoseDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string DailyDoseId { get; set; } = string.Empty;
    public string TreatmentId { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}