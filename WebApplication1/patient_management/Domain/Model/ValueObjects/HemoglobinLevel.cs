namespace WebApplication1.patient_management.Domain.ValueObjects;

/// <summary>
/// Value Object para Nivel de Hemoglobina (g/dL)
/// </summary>
public class HemoglobinLevel
{
    public double? Value { get; }

    public HemoglobinLevel(double? value)
    {
        if (value.HasValue)
        {
            if (value < 0 || value > 30)
                throw new ArgumentException("Hemoglobin level must be between 0 and 30 g/dL", nameof(value));

            Value = Math.Round(value.Value, 2);
        }
        else
        {
            Value = null;
        }
    }

    // Constructor privado para serialización
    private HemoglobinLevel() { }

    public bool HasValue() => Value.HasValue;

    public override bool Equals(object? obj)
    {
        return obj is HemoglobinLevel other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    public override string ToString()
    {
        return Value.HasValue ? $"{Value.Value} g/dL" : "N/A";
    }

    public static implicit operator double?(HemoglobinLevel level) => level.Value;
}