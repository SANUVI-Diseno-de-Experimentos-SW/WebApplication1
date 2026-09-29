using System.Text.RegularExpressions;

namespace WebApplication1.Contexts.IAM.Domain.Models.ValueObjects;
using System.Text.RegularExpressions;


/// <summary>
/// Value Object para el Teléfono (formato Perú: +51 987654321)
/// </summary>
public class Phone
{
    public string Value { get; }

    public Phone(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone is required", nameof(value));

        // ✅ NORMALIZAR: Eliminar espacios, guiones, etc.
        var normalized = Normalize(value);

        // ✅ VALIDAR: Aceptar tanto formato +51 987654321 como solo números (987654321)
        if (!IsValidPhone(normalized))
        {
            throw new ArgumentException(
                "Phone must follow format: +51 987654321 (or just 987654321)", 
                nameof(value));
        }

        Value = normalized;
    }

    // Constructor privado para serialización
    private Phone() { }

    private static string Normalize(string phone)
    {
        // Eliminar espacios, guiones, paréntesis
        return Regex.Replace(phone, @"[\s\-\(\)]", "");
    }

    private static bool IsValidPhone(string phone)
    {
        // ✅ Aceptar: +51987654321 (con código país) o 987654321 (sin código país)
        // O también: 51987654321
        return Regex.IsMatch(phone, @"^(\+?51)?\d{9}$");
    }

    /// <summary>
    /// Factory method para deserialización desde MongoDB
    /// </summary>
    public static Phone FromPersistence(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required", nameof(phone));

        // ✅ Si el teléfono viene solo con números, le agregamos el +51
        var normalized = Normalize(phone);
        
        // Si son solo 9 dígitos, agregar +51
        if (Regex.IsMatch(normalized, @"^\d{9}$"))
        {
            normalized = $"+51{normalized}";
        }
        // Si son 11 dígitos (51 + 9 dígitos), agregar +
        else if (Regex.IsMatch(normalized, @"^51\d{9}$"))
        {
            normalized = $"+{normalized}";
        }

        return new Phone(normalized);
    }

    public override bool Equals(object? obj)
    {
        return obj is Phone other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(Phone phone) => phone.Value;
}