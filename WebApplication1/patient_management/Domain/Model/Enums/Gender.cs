namespace WebApplication1.patient_management.Domain.Enums;

using System.Runtime.Serialization;


/// <summary>
/// Género del paciente
/// </summary>
public enum Gender
{
    [EnumMember(Value = "MALE")]
    Male,

    [EnumMember(Value = "FEMALE")]
    Female
}

/// <summary>
/// Extensiones para Gender
/// </summary>
public static class GenderExtensions
{
    public static string ToStringValue(this Gender gender)
    {
        return gender switch
        {
            Gender.Male => "MALE",
            Gender.Female => "FEMALE",
            _ => throw new ArgumentOutOfRangeException(nameof(gender), gender, null)
        };
    }

    public static Gender FromString(string value)
    {
        return value switch
        {
            "MALE" => Gender.Male,
            "FEMALE" => Gender.Female,
            _ => throw new ArgumentException($"Invalid gender: {value}")
        };
    }

    public static string GetDisplayName(this Gender gender)
    {
        return gender switch
        {
            Gender.Male => "Masculino",
            Gender.Female => "Femenino",
            _ => gender.ToString()
        };
    }
}