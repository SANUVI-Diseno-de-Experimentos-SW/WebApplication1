namespace WebApplication1.Consultation.Infrastructure.Persitencia.MongoDb.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class ConsultationDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string ConsultationId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string MotherId { get; set; } = string.Empty;
    public string NurseId { get; set; } = string.Empty;
    public List<MessageDocument> Messages { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}