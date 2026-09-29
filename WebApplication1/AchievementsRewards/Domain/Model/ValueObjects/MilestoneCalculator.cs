namespace WebApplication1.AchievementsRewards.Domain.Model.ValueObjects;

public static class MilestoneCalculator
{
    public static int GetMilestone(BadgeType type, int durationDays)
    {
        return type switch
        {
            BadgeType.FIRST_WEEK => 7,
            BadgeType.HALF_TREATMENT => (int)Math.Ceiling(durationDays / 2.0),
            BadgeType.TREATMENT_COMPLETED => durationDays,
            _ => throw new ArgumentException($"Unknown badge type: {type}")
        };
    }

    public static List<BadgeType> GetBadgesForDuration(int durationDays)
    {
        var badges = new List<BadgeType>();

        // FIRST_WEEK: solo si dura al menos 7 días
        if (durationDays >= 7)
        {
            badges.Add(BadgeType.FIRST_WEEK);
        }

        // HALF_TREATMENT: solo si la mitad es mayor a 7 y menor que durationDays
        var halfMilestone = (int)Math.Ceiling(durationDays / 2.0);
        if (durationDays >= 30 && halfMilestone > 7 && halfMilestone < durationDays)
        {
            badges.Add(BadgeType.HALF_TREATMENT);
        }

        // TREATMENT_COMPLETED: siempre
        badges.Add(BadgeType.TREATMENT_COMPLETED);

        return badges;
    }
}