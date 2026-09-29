namespace WebApplication1.patient_management.Domain.ValueObjects;

/// <summary>
/// Value Object para Motivo de Consulta
/// </summary>
public class MotivoConsulta
{
    public string Value { get; }

    public MotivoConsulta(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Consultation reason is required", nameof(value));

        if (value.Trim().Length < 5)
            throw new ArgumentException("Consultation reason must contain at least 5 characters", nameof(value));

        Value = value.Trim();
    }

    // Constructor privado para serialización
    private MotivoConsulta() { }

    public override bool Equals(object? obj)
    {
        return obj is MotivoConsulta other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(MotivoConsulta motivo) => motivo.Value;
}