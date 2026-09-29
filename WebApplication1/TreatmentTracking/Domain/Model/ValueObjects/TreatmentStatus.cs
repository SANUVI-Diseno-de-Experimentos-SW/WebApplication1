namespace WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;

public enum TreatmentStatus
{
    ACTIVE,
    COMPLETED,
    ABANDONED
}

public static class TreatmentStatusExtensions
{
    public static string ToStringValue(this TreatmentStatus status)
    {
        return status switch
        {
            TreatmentStatus.ACTIVE => "ACTIVE",
            TreatmentStatus.COMPLETED => "COMPLETED",
            TreatmentStatus.ABANDONED => "ABANDONED",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static TreatmentStatus FromString(string value)
    {
        return value switch
        {
            "ACTIVE" => TreatmentStatus.ACTIVE,
            "COMPLETED" => TreatmentStatus.COMPLETED,
            "ABANDONED" => TreatmentStatus.ABANDONED,
            _ => throw new ArgumentException($"Invalid treatment status: {value}")
        };
    }

    public static bool IsActive(this TreatmentStatus status) => status == TreatmentStatus.ACTIVE;
    public static bool IsCompleted(this TreatmentStatus status) => status == TreatmentStatus.COMPLETED;
    public static bool IsAbandoned(this TreatmentStatus status) => status == TreatmentStatus.ABANDONED;
}