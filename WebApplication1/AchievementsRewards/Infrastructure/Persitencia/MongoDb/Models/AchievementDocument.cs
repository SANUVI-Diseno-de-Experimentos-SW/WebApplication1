namespace WebApplication1.AchievementsRewards.Infrastructure.Persitencia.MongoDb.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class AchievementDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string AchievementId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string MotherId { get; set; } = string.Empty;
    public string TreatmentId { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int BestStreak { get; set; }
    public DateTime? StreakStartDate { get; set; }
    public int TotalPoints { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}