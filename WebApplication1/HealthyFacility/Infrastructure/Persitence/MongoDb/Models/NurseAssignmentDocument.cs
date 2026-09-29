using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Models;

public class NurseAssignmentDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string NurseAssignmentId { get; set; } = string.Empty;
    public string FacilityId { get; set; } = string.Empty;
    public string NurseId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}