namespace WebApplication1.patient_management.Domain.Enums;

using System.Runtime.Serialization;


/// <summary>
/// Estado del paciente
/// </summary>
public enum PatientStatus
{
    [EnumMember(Value = "ACTIVE")]
    Active,

    [EnumMember(Value = "INACTIVE")]
    Inactive,

    [EnumMember(Value = "DISCHARGED")]
    Discharged
}

/// <summary>
/// Extensiones para PatientStatus
/// </summary>
public static class PatientStatusExtensions
{
    public static string ToStringValue(this PatientStatus status)
    {
        return status switch
        {
            PatientStatus.Active => "ACTIVE",
            PatientStatus.Inactive => "INACTIVE",
            PatientStatus.Discharged => "DISCHARGED",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static PatientStatus FromString(string value)
    {
        return value switch
        {
            "ACTIVE" => PatientStatus.Active,
            "INACTIVE" => PatientStatus.Inactive,
            "DISCHARGED" => PatientStatus.Discharged,
            _ => throw new ArgumentException($"Invalid patient status: {value}")
        };
    }

    public static string GetDisplayName(this PatientStatus status)
    {
        return status switch
        {
            PatientStatus.Active => "Activo",
            PatientStatus.Inactive => "Inactivo",
            PatientStatus.Discharged => "Dado de alta",
            _ => status.ToString()
        };
    }

    public static bool IsActive(this PatientStatus status) => status == PatientStatus.Active;
    public static bool IsDischarged(this PatientStatus status) => status == PatientStatus.Discharged;
    public static bool CanBeAssigned(this PatientStatus status) => status == PatientStatus.Active || status == PatientStatus.Inactive;
}