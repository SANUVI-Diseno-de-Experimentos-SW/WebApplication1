namespace WebApplication1.patient_management.Domain.ValueObjects;


/// <summary>
/// Value Object para Altura (en cm)
/// </summary>
public class Height
{
    public double Value { get; }

    public Height(double value)
    {
        if (value <= 0)
            throw new ArgumentException("Height must be greater than zero", nameof(value));

        if (value > 300)
            throw new ArgumentException("Height must be less than 300 cm", nameof(value));

        Value = value;
    }

    // Constructor privado para serialización
    private Height() { }

    public override bool Equals(object? obj)
    {
        return obj is Height other && Math.Abs(Value - other.Value) < 0.01;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Value} cm";
    }

    public static implicit operator double(Height height) => height.Value;
}