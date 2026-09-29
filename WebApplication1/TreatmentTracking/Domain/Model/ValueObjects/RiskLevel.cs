namespace WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;
public enum RiskLevel
{
    LOW,
    MEDIUM,
    HIGH
}

public static class RiskLevelExtensions
{
    public static string ToStringValue(this RiskLevel level)
    {
        return level switch
        {
            RiskLevel.LOW => "LOW",
            RiskLevel.MEDIUM => "MEDIUM",
            RiskLevel.HIGH => "HIGH",
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }

    public static RiskLevel FromString(string value)
    {
        return value switch
        {
            "LOW" => RiskLevel.LOW,
            "MEDIUM" => RiskLevel.MEDIUM,
            "HIGH" => RiskLevel.HIGH,
            _ => throw new ArgumentException($"Invalid risk level: {value}")
        };
    }

    public static string GetDisplayName(this RiskLevel level)
    {
        return level switch
        {
            RiskLevel.LOW => "Bajo",
            RiskLevel.MEDIUM => "Medio",
            RiskLevel.HIGH => "Alto",
            _ => level.ToString()
        };
    }
}