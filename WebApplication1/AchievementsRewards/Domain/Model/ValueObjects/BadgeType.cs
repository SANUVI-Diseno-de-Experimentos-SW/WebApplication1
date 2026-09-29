namespace WebApplication1.AchievementsRewards.Domain.Model.ValueObjects;


public enum BadgeType
{
    FIRST_WEEK,
    HALF_TREATMENT,
    TREATMENT_COMPLETED
}

public static class BadgeTypeExtensions
{
    public static string ToStringValue(this BadgeType type)
    {
        return type switch
        {
            BadgeType.FIRST_WEEK => "FIRST_WEEK",
            BadgeType.HALF_TREATMENT => "HALF_TREATMENT",
            BadgeType.TREATMENT_COMPLETED => "TREATMENT_COMPLETED",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static BadgeType FromString(string value)
    {
        return value switch
        {
            "FIRST_WEEK" => BadgeType.FIRST_WEEK,
            "HALF_TREATMENT" => BadgeType.HALF_TREATMENT,
            "TREATMENT_COMPLETED" => BadgeType.TREATMENT_COMPLETED,
            _ => throw new ArgumentException($"Invalid badge type: {value}")
        };
    }
}