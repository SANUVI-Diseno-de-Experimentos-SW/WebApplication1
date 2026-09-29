using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Models;


public class AppointmentDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string AppointmentId { get; set; } = string.Empty;
    public string FacilityId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string MotherId { get; set; } = string.Empty;
    public string? NurseId { get; set; }
    public string AppointmentDate { get; set; } = string.Empty;
    public string AppointmentTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}