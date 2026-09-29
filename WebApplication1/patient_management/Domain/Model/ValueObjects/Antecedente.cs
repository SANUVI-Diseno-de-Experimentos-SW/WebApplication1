namespace WebApplication1.patient_management.Domain.ValueObjects;

/// <summary>
/// Value Object para Antecedentes médicos
/// </summary>
public class Antecedente
{
    public string Type { get; }
    public string Description { get; }

    public Antecedente(string type, string description)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Antecedent type is required", nameof(type));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Antecedent description is required", nameof(description));

        Type = type.Trim();
        Description = description.Trim();
    }

    // Constructor privado para serialización
    private Antecedente() { }

    public AntecedentePrimitives ToPrimitives()
    {
        return new AntecedentePrimitives
        {
            Type = Type,
            Description = Description
        };
    }

    public override bool Equals(object? obj)
    {
        return obj is Antecedente other && Type == other.Type && Description == other.Description;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Description);
    }

    public class AntecedentePrimitives
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}