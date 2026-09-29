using System.Text.RegularExpressions;

namespace WebApplication1.Contexts.IAM.Domain.Models.ValueObjects;

/// <summary>
/// Value Object para el DNI (Documento Nacional de Identidad)
/// </summary>
public class Dni
{
    public string Value { get; }

    public Dni(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("DNI is required", nameof(value));

        // Validar: exactamente 8 dígitos
        if (!Regex.IsMatch(value, @"^\d{8}$"))
            throw new ArgumentException("DNI must contain exactly 8 numeric digits", nameof(value));

        Value = value;
    }

    // Constructor privado para serialización
    private Dni() { }

    public override bool Equals(object? obj)
    {
        return obj is Dni other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(Dni dni) => dni.Value;
}