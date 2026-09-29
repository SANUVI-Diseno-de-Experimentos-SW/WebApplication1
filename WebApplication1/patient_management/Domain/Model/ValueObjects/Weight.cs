namespace WebApplication1.patient_management.Domain.ValueObjects;


/// <summary>
/// Value Object para Peso (en kg)
/// </summary>
public class Weight
{
    public double Value { get; }

    public Weight(double value)
    {
        if (value <= 0)
            throw new ArgumentException("Weight must be greater than zero", nameof(value));

        if (value > 500)
            throw new ArgumentException("Weight must be less than 500 kg", nameof(value));

        Value = Math.Round(value, 2);
    }

    // Constructor privado para serialización
    private Weight() { }

    public override bool Equals(object? obj)
    {
        return obj is Weight other && Math.Abs(Value - other.Value) < 0.01;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Value} kg";
    }

    public static implicit operator double(Weight weight) => weight.Value;
}