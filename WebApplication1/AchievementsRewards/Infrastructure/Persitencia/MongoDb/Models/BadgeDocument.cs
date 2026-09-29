namespace WebApplication1.AchievementsRewards.Infrastructure.Persitencia.MongoDb.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class BadgeDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string BadgeId { get; set; } = string.Empty;
    public string AchievementId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Milestone { get; set; }
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}