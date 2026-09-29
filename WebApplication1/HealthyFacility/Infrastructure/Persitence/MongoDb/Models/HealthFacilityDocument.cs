using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Models;

public class CoordinatesDocument
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}

public class OperatingScheduleDocument
{
    public List<string> AvailableDays { get; set; } = new();
    public List<string> AvailableSlots { get; set; } = new();
}

public class HealthFacilityDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string HealthFacilityId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DistrictId { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public CoordinatesDocument Coordinates { get; set; } = new();
    public string PhoneNumber { get; set; } = string.Empty;
    public List<string> Services { get; set; } = new();
    public OperatingScheduleDocument OperatingSchedule { get; set; } = new();
    public string ScheduleOfOperation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    // ✅ AGREGAR ESTO
    public List<NurseAssignmentDocument> NurseAssignments { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}