using WebApplication1.shared.catalogs.Domain;

namespace WebApplication1.shared.catalogs.Data;



/// <summary>
/// Datos semilla de distritos del Perú
/// </summary>
public static class DistrictSeed
{
    public static IReadOnlyList<District> Districts { get; } = new List<District>
    {
        new District("DIST001", "San Juan de Lurigancho"),
        new District("DIST002", "Ate"),
        new District("DIST003", "Villa El Salvador"),
        new District("DIST004", "Comas"),
        new District("DIST005", "San Martín de Porres"),
        new District("DIST006", "Callao"),
        new District("DIST007", "San Miguel"),
        new District("DIST008", "Miraflores"),
        new District("DIST009", "Surco"),
        new District("DIST010", "La Molina"),
        new District("DIST011", "San Borja"),
        new District("DIST012", "San Isidro"),
        new District("DIST013", "Barranco"),
        new District("DIST014", "Chorrillos"),
        new District("DIST015", "Villa María del Triunfo"),
        new District("DIST016", "Puente Piedra"),
        new District("DIST017", "Los Olivos"),
        new District("DIST018", "Independencia"),
        new District("DIST019", "Rímac"),
        new District("DIST020", "Cercado de Lima"),
    };

    // Método para obtener todos los distritos
    public static IEnumerable<District> GetAll() => Districts;

    // Método para buscar por ID
    public static District? FindById(string id) => 
        Districts.FirstOrDefault(d => d.Id == id);

    // Método para buscar por nombre (case insensitive)
    public static District? FindByName(string name) =>
        Districts.FirstOrDefault(d => 
            d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    // Método para buscar por parte del nombre
    public static IEnumerable<District> Search(string searchTerm) =>
        Districts.Where(d => 
            d.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
}