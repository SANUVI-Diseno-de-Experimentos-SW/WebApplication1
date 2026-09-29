using WebApplication1.AchievementsRewards.Domain.Model.Aggregate;
using WebApplication1.AchievementsRewards.Domain.Model.Entities;
using WebApplication1.AchievementsRewards.Domain.Model.Events;

namespace WebApplication1.AchievementsRewards.Domain.Services;


public class AchievementEvaluatorService
{
    public (List<Badge> UpdatedBadges, List<BadgeUnlockedEvent> Events) EvaluateBadges(
        Achievement achievement,
        List<Badge> badges)
    {
        var unlockedBadges = new List<Badge>();
        var events = new List<BadgeUnlockedEvent>();
        var bestStreak = achievement.BestStreak;

        // Ordenar badges por milestone (menor a mayor)
        var sortedBadges = badges.OrderBy(b => b.Milestone).ToList();

        foreach (var badge in sortedBadges)
        {
            if (badge.IsUnlocked)
            {
                continue;
            }

            if (badge.CanBeUnlockedWithBestStreak(bestStreak))
            {
                badge.Unlock();
                unlockedBadges.Add(badge);

                var eventObj = new BadgeUnlockedEvent(
                    achievement.MotherId,
                    achievement.PatientId,
                    achievement.TreatmentId,
                    badge.Id,
                    badge.Type,
                    badge.Name,
                    badge.Milestone,
                    badge.UnlockedAt!.Value
                );

                events.Add(eventObj);
            }
        }

        return (unlockedBadges, events);
    }

    public StreakMilestoneReachedEvent? EvaluateStreakMilestone(
        Achievement achievement,
        int previousStreak,
        int currentStreak)
    {
        int[] milestones = { 7, 15, 30, 60, 90, 120, 150, 180, 365 };

        foreach (var milestone in milestones)
        {
            if (previousStreak < milestone && currentStreak >= milestone)
            {
                return new StreakMilestoneReachedEvent(
                    achievement.MotherId,
                    achievement.PatientId,
                    achievement.TreatmentId,
                    currentStreak,
                    milestone
                );
            }
        }

        return null;
    }

    public int CalculatePoints(string eventType)
    {
        return eventType switch
        {
            "DOSE_CONFIRMED" => 10,
            "TREATMENT_COMPLETED" => 50,
            _ => 0
        };
    }

    public PointsEarnedEvent CreatePointsEvent(
        Achievement achievement,
        int pointsEarned,
        string reason)
    {
        return new PointsEarnedEvent(
            achievement.MotherId,
            achievement.PatientId,
            achievement.TreatmentId,
            pointsEarned,
            achievement.TotalPoints,
            reason
        );
    }

    public bool IsTreatmentComplete(int totalConfirmedDoses, int durationDays)
    {
        return totalConfirmedDoses >= durationDays;
    }
}