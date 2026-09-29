namespace WebApplication1.HealthyFacility.Domain.Models.ValueObjects;


public enum FacilityStatus
{
    ACTIVE,
    INACTIVE
}

public static class FacilityStatusExtensions
{
    public static string ToStringValue(this FacilityStatus status)
    {
        return status switch
        {
            FacilityStatus.ACTIVE => "ACTIVE",
            FacilityStatus.INACTIVE => "INACTIVE",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static FacilityStatus FromString(string value)
    {
        return value switch
        {
            "ACTIVE" => FacilityStatus.ACTIVE,
            "INACTIVE" => FacilityStatus.INACTIVE,
            _ => throw new ArgumentException($"Invalid facility status: {value}")
        };
    }

    public static bool IsActive(this FacilityStatus status) => status == FacilityStatus.ACTIVE;
    public static bool IsInactive(this FacilityStatus status) => status == FacilityStatus.INACTIVE;
}