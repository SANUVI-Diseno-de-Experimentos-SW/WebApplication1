using WebApplication1.AchievementsRewards.Domain.Model.ValueObjects;

namespace WebApplication1.AchievementsRewards.Domain.Model.Events;



public class BadgeUnlockedEvent
{
    public string EventName => "BadgeUnlocked";
    public DateTime OccurredAt { get; }

    public string MotherId { get; }
    public string PatientId { get; }
    public string TreatmentId { get; }
    public string BadgeId { get; }
    public BadgeType BadgeType { get; }
    public string BadgeName { get; }
    public int Milestone { get; }
    public DateTime UnlockedAt { get; }

    public BadgeUnlockedEvent(
        string motherId,
        string patientId,
        string treatmentId,
        string badgeId,
        BadgeType badgeType,
        string badgeName,
        int milestone,
        DateTime unlockedAt)
    {
        MotherId = motherId;
        PatientId = patientId;
        TreatmentId = treatmentId;
        BadgeId = badgeId;
        BadgeType = badgeType;
        BadgeName = badgeName;
        Milestone = milestone;
        UnlockedAt = unlockedAt;
        OccurredAt = DateTime.UtcNow;
    }

    public object ToPrimitives()
    {
        return new
        {
            eventName = EventName,
            occurredAt = OccurredAt,
            data = new
            {
                motherId = MotherId,
                patientId = PatientId,
                treatmentId = TreatmentId,
                badgeId = BadgeId,
                badgeType = BadgeType.ToStringValue(),
                badgeName = BadgeName,
                milestone = Milestone,
                unlockedAt = UnlockedAt
            }
        };
    }
}