namespace WebApplication1.AchievementsRewards.Domain.Model.Events;


public class PointsEarnedEvent
{
    public string EventName => "PointsEarned";
    public DateTime OccurredAt { get; }

    public string MotherId { get; }
    public string PatientId { get; }
    public string TreatmentId { get; }
    public int PointsEarned { get; }
    public int TotalPoints { get; }
    public string Reason { get; }

    public PointsEarnedEvent(
        string motherId,
        string patientId,
        string treatmentId,
        int pointsEarned,
        int totalPoints,
        string reason)
    {
        MotherId = motherId;
        PatientId = patientId;
        TreatmentId = treatmentId;
        PointsEarned = pointsEarned;
        TotalPoints = totalPoints;
        Reason = reason;
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
                pointsEarned = PointsEarned,
                totalPoints = TotalPoints,
                reason = Reason
            }
        };
    }
}