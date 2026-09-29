namespace WebApplication1.patient_management.Domain.ValueObjects;


/// <summary>
/// Value Object para Fecha de Nacimiento
/// </summary>
public class BirthDate
{
    public DateTime Value { get; }

    public BirthDate(DateTime value)
    {
        if (value == default)
            throw new ArgumentException("Birth date is required", nameof(value));

        if (value > DateTime.UtcNow)
            throw new ArgumentException("Birth date cannot be in the future", nameof(value));

        if (value < DateTime.UtcNow.AddYears(-120))
            throw new ArgumentException("Invalid birth date", nameof(value));

        Value = value;
    }

    // Constructor privado para serialización
    private BirthDate() { }

    public int GetAge()
    {
        var today = DateTime.UtcNow;
        var age = today.Year - Value.Year;
        if (Value.Date > today.AddYears(-age)) age--;
        return age;
    }

    public bool IsMinor() => GetAge() < 18;
    public bool IsAdult() => GetAge() >= 18;
    public bool IsElderly() => GetAge() >= 65;

    public override bool Equals(object? obj)
    {
        return obj is BirthDate other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value.ToString("yyyy-MM-dd");
    }

    public static implicit operator DateTime(BirthDate birthDate) => birthDate.Value;
}