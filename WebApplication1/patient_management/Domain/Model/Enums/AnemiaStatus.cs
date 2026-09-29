using System.Runtime.Serialization;

namespace WebApplication1.patient_management.Domain.Enums;

using System.Runtime.Serialization;


/// <summary>
/// Estado de Anemia basado en niveles de hemoglobina
/// </summary>
public enum AnemiaStatus
{
    [EnumMember(Value = "MILD")]
    Mild,

    [EnumMember(Value = "MODERATE")]
    Moderate,

    [EnumMember(Value = "SEVERE")]
    Severe,

    [EnumMember(Value = "CONTROLLED")]
    Controlled
}

/// <summary>
/// Extensiones para AnemiaStatus
/// </summary>
public static class AnemiaStatusExtensions
{
    public static string ToStringValue(this AnemiaStatus status)
    {
        return status switch
        {
            AnemiaStatus.Mild => "MILD",
            AnemiaStatus.Moderate => "MODERATE",
            AnemiaStatus.Severe => "SEVERE",
            AnemiaStatus.Controlled => "CONTROLLED",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static AnemiaStatus FromString(string value)
    {
        return value switch
        {
            "MILD" => AnemiaStatus.Mild,
            "MODERATE" => AnemiaStatus.Moderate,
            "SEVERE" => AnemiaStatus.Severe,
            "CONTROLLED" => AnemiaStatus.Controlled,
            _ => throw new ArgumentException($"Invalid anemia status: {value}")
        };
    }

    public static string GetDescription(this AnemiaStatus status)
    {
        return status switch
        {
            AnemiaStatus.Mild => "Leve",
            AnemiaStatus.Moderate => "Moderada",
            AnemiaStatus.Severe => "Severa",
            AnemiaStatus.Controlled => "Controlada",
            _ => status.ToString()
        };
    }
}