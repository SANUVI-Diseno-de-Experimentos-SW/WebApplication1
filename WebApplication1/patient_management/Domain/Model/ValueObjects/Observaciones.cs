namespace WebApplication1.patient_management.Domain.ValueObjects;


/// <summary>
/// Value Object para Observaciones
/// </summary>
public class Observaciones
{
    public string? Value { get; }

    public Observaciones(string? value)
    {
        if (value != null && string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Observations cannot be empty", nameof(value));

        Value = value?.Trim();
    }

    // Constructor privado para serialización
    private Observaciones() { }

    public bool IsEmpty() => string.IsNullOrWhiteSpace(Value);

    public override bool Equals(object? obj)
    {
        return obj is Observaciones other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    public override string ToString()
    {
        return Value ?? string.Empty;
    }

    public static implicit operator string?(Observaciones observaciones) => observaciones.Value;
}