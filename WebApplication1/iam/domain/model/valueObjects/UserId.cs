namespace WebApplication1.Contexts.IAM.Domain.Models.ValueObjects;

/// <summary>
/// Value Object para el ID de usuario
/// </summary>
public class UserId
{
    public string Value { get; }

    public UserId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("User id is required", nameof(value));

        Value = value;
    }

    // Constructor privado para serialización (MongoDB/EF)
    private UserId() { }

    public override bool Equals(object? obj)
    {
        return obj is UserId other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    // Operador implícito para convertir a string
    public static implicit operator string(UserId userId) => userId.Value;
}