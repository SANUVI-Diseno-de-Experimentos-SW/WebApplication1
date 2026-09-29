namespace WebApplication1.AchievementsRewards.Domain.Model.Events;


public class StreakMilestoneReachedEvent
{
    public string EventName => "StreakMilestoneReached";
    public DateTime OccurredAt { get; }

    public string MotherId { get; }
    public string PatientId { get; }
    public string TreatmentId { get; }
    public int CurrentStreak { get; }
    public int Milestone { get; }

    public StreakMilestoneReachedEvent(
        string motherId,
        string patientId,
        string treatmentId,
        int currentStreak,
        int milestone)
    {
        MotherId = motherId;
        PatientId = patientId;
        TreatmentId = treatmentId;
        CurrentStreak = currentStreak;
        Milestone = milestone;
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
                currentStreak = CurrentStreak,
                milestone = Milestone
            }
        };
    }
}