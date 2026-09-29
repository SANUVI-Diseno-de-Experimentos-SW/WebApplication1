using System.Text.RegularExpressions;

namespace WebApplication1.Contexts.IAM.Domain.Models.ValueObjects;

/// <summary>
/// Value Object para el Email
/// </summary>
public class Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email is required", nameof(value));

        // Validar formato de email
        var regex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
        if (!regex.IsMatch(value))
            throw new ArgumentException("Invalid email format", nameof(value));

        Value = value.ToLowerInvariant();
    }

    // Constructor privado para serialización
    private Email() { }

    public override bool Equals(object? obj)
    {
        return obj is Email other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(Email email) => email.Value;
}