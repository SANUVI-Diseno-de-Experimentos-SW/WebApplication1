using WebApplication1.shared.catalogs.Domain;

namespace WebApplication1.shared.catalogs.Data;



/// <summary>
/// Repositorio de distritos
/// </summary>
public class DistrictRepository
{
    private readonly IReadOnlyList<District> _districts;

    public DistrictRepository()
    {
        _districts = DistrictSeed.Districts;
    }

    /// <summary>
    /// Obtiene todos los distritos
    /// </summary>
    public IReadOnlyList<District> FindAll() => _districts;

    /// <summary>
    /// Obtiene todos los distritos (versión IEnumerable)
    /// </summary>
    public IEnumerable<District> GetAll() => _districts;

    /// <summary>
    /// Busca un distrito por su ID
    /// </summary>
    public District? FindById(string id) =>
        _districts.FirstOrDefault(d => d.Id == id);

    /// <summary>
    /// Busca distritos por nombre (coincidencia exacta)
    /// </summary>
    public District? FindByName(string name) =>
        _districts.FirstOrDefault(d => 
            d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Busca distritos que contengan el término de búsqueda
    /// </summary>
    public IEnumerable<District> Search(string searchTerm) =>
        _districts.Where(d => 
            d.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Verifica si existe un distrito con el ID dado
    /// </summary>
    public bool Exists(string id) =>
        _districts.Any(d => d.Id == id);

    /// <summary>
    /// Obtiene la cantidad total de distritos
    /// </summary>
    public int Count() => _districts.Count;

    /// <summary>
    /// Obtiene distritos paginados
    /// </summary>
    public IEnumerable<District> GetPaged(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        return _districts
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }
}