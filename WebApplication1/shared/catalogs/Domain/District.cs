namespace WebApplication1.shared.catalogs.Domain;


/// <summary>
/// Representa un distrito del Perú
/// </summary>
public class District
{
    public string Id { get; }
    public string Name { get; }

    public District(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("El ID del distrito no puede estar vacío", nameof(id));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del distrito no puede estar vacío", nameof(name));

        Id = id;
        Name = name;
    }

    // Para compatibilidad con EF Core / MongoDB (si necesitas)
    private District() { } // Constructor privado para serialización

    public override string ToString() => $"{Name} ({Id})";
    public override bool Equals(object? obj) => obj is District other && Id == other.Id;
    public override int GetHashCode() => Id.GetHashCode();
}