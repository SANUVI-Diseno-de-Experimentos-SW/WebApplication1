using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.TreatmentTracking.Infrastructure.Persitencia.MongoDb.Models;

public class RiskScoreDocument
{
    [BsonElement("id")]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("score")]
    public int Score { get; set; }
    
    [BsonElement("riskLevel")]
    public string RiskLevel { get; set; } = string.Empty;
    
    [BsonElement("calculatedAt")]
    public DateTime CalculatedAt { get; set; }
}

public class TreatmentDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("id")]
    public string TreatmentId { get; set; } = string.Empty;
    
    [BsonElement("patientId")]
    public string PatientId { get; set; } = string.Empty;
    
    [BsonElement("nurseId")]
    public string NurseId { get; set; } = string.Empty;
    
    [BsonElement("supplement")]
    public string Supplement { get; set; } = string.Empty;
    
    [BsonElement("quantity")]
    public string Quantity { get; set; } = string.Empty;
    
    [BsonElement("dosingHours")]
    public string DosingHours { get; set; } = string.Empty;
    
    [BsonElement("durationDays")]
    public int DurationDays { get; set; }
    
    [BsonElement("startDate")]
    public DateTime StartDate { get; set; }
    
    [BsonElement("endDate")]
    public DateTime EndDate { get; set; }
    
    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;
    
    [BsonElement("adherenceScore")]
    public double AdherenceScore { get; set; }
    
    [BsonElement("currentStreak")]
    public int CurrentStreak { get; set; }
    
    [BsonElement("totalConfirmed")]
    public int TotalConfirmed { get; set; }
    
    [BsonElement("totalOmitted")]
    public int TotalOmitted { get; set; }
    
    [BsonElement("completionObservation")]
    public string? CompletionObservation { get; set; }
    
    [BsonElement("abandonmentObservation")]
    public string? AbandonmentObservation { get; set; }
    
    [BsonElement("riskScore")]
    public RiskScoreDocument RiskScore { get; set; } = new();
}