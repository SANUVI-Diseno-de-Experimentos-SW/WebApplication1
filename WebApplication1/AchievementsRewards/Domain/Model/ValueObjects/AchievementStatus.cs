namespace WebApplication1.AchievementsRewards.Domain.Model.ValueObjects;


public enum AchievementStatus
{
    ACTIVE,
    COMPLETED,
    ABANDONED
}

public static class AchievementStatusExtensions
{
    public static string ToStringValue(this AchievementStatus status)
    {
        return status switch
        {
            AchievementStatus.ACTIVE => "ACTIVE",
            AchievementStatus.COMPLETED => "COMPLETED",
            AchievementStatus.ABANDONED => "ABANDONED",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static AchievementStatus FromString(string value)
    {
        return value switch
        {
            "ACTIVE" => AchievementStatus.ACTIVE,
            "COMPLETED" => AchievementStatus.COMPLETED,
            "ABANDONED" => AchievementStatus.ABANDONED,
            _ => throw new ArgumentException($"Invalid achievement status: {value}")
        };
    }
}