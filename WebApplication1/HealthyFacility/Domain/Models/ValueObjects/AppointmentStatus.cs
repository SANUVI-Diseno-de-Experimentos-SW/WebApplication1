namespace WebApplication1.HealthyFacility.Domain.Models.ValueObjects;

public enum AppointmentStatus
{
    CONFIRMED,
    CANCELLED
}

public static class AppointmentStatusExtensions
{
    public static string ToStringValue(this AppointmentStatus status)
    {
        return status switch
        {
            AppointmentStatus.CONFIRMED => "CONFIRMED",
            AppointmentStatus.CANCELLED => "CANCELLED",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static AppointmentStatus FromString(string value)
    {
        return value switch
        {
            "CONFIRMED" => AppointmentStatus.CONFIRMED,
            "CANCELLED" => AppointmentStatus.CANCELLED,
            _ => throw new ArgumentException($"Invalid appointment status: {value}")
        };
    }

    public static bool IsConfirmed(this AppointmentStatus status) => status == AppointmentStatus.CONFIRMED;
    public static bool IsCancelled(this AppointmentStatus status) => status == AppointmentStatus.CANCELLED;
}