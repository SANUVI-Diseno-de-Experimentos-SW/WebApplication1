namespace WebApplication1.TreatmentTracking.Domain.Model.ValueObjects;
public enum DoseStatus
{
    PENDING,
    CONFIRMED,
    OMITTED
}

public static class DoseStatusExtensions
{
    public static string ToStringValue(this DoseStatus status)
    {
        return status switch
        {
            DoseStatus.PENDING => "PENDING",
            DoseStatus.CONFIRMED => "CONFIRMED",
            DoseStatus.OMITTED => "OMITTED",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static DoseStatus FromString(string value)
    {
        return value switch
        {
            "PENDING" => DoseStatus.PENDING,
            "CONFIRMED" => DoseStatus.CONFIRMED,
            "OMITTED" => DoseStatus.OMITTED,
            _ => throw new ArgumentException($"Invalid dose status: {value}")
        };
    }

    public static bool IsPending(this DoseStatus status) => status == DoseStatus.PENDING;
    public static bool IsConfirmed(this DoseStatus status) => status == DoseStatus.CONFIRMED;
    public static bool IsOmitted(this DoseStatus status) => status == DoseStatus.OMITTED;
}