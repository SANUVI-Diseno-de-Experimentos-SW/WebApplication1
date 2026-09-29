using System.Text.RegularExpressions;

namespace WebApplication1.Contexts.IAM.Domain.Models.ValueObjects;

/// <summary>
/// Value Object para la contraseña
/// </summary>
public class Password
{
    public string Value { get; }

    public Password(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Password is required", nameof(value));

        // Validar: mínimo 8 caracteres, mayúscula, minúscula, número y símbolo
        var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$");
        if (!regex.IsMatch(value))
            throw new ArgumentException(
                "Password must contain uppercase, lowercase, number and symbol",
                nameof(value)
            );

        Value = value;
    }

    // Constructor privado para serialización
    private Password() { }

    public override bool Equals(object? obj)
    {
        return obj is Password other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return "********"; // Nunca mostrar la contraseña real
    }

    public static implicit operator string(Password password) => password.Value;
}