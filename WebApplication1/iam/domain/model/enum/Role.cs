using System.Runtime.Serialization;

namespace WebApplication1.Contexts.IAM.Domain.Models.Enums;

/// <summary>
/// Roles de usuario en el sistema
/// </summary>
public enum Role
{
    [EnumMember(Value = "Mother")]
    Mother,

    [EnumMember(Value = "Nurse")]
    Nurse,

    [EnumMember(Value = "Admin")]
    Admin
}

/// <summary>
/// Extensiones para el enum Role
/// </summary>
public static class RoleExtensions
{
    public static string ToStringValue(this Role role)
    {
        return role switch
        {
            Role.Mother => "Mother",
            Role.Nurse => "Nurse",
            Role.Admin => "Admin",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }

    public static Role FromString(string value)
    {
        return value switch
        {
            "Mother" => Role.Mother,
            "Nurse" => Role.Nurse,
            "Admin" => Role.Admin,
            _ => throw new ArgumentException($"Invalid role: {value}")
        };
    }

    public static bool IsStaff(this Role role)
    {
        return role == Role.Nurse || role == Role.Admin;
    }

    public static bool IsMother(this Role role)
    {
        return role == Role.Mother;
    }

    public static bool CanManageUsers(this Role role)
    {
        return role == Role.Admin;
    }

    public static bool CanAccessMedicalRecords(this Role role)
    {
        return role == Role.Nurse || role == Role.Admin;
    }
}