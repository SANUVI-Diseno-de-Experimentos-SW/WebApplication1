using WebApplication1.HealthyFacility.Domain.Models.Aggregate;

namespace WebApplication1.HealthyFacility.Domain.Repositories;



public interface IHealthFacilityRepository
{
    /// <summary>
    /// Saves the health facility to the repository.
    /// </summary>
    Task<HealthFacility> SaveAsync(HealthFacility facility);

    /// <summary>
    /// Finds a health facility by its ID.
    /// </summary>
    Task<HealthFacility?> FindByIdAsync(string id);

    /// <summary>
    /// Finds all health facilities in the repository.
    /// </summary>
    Task<List<HealthFacility>> FindAllAsync();

    /// <summary>
    /// Finds all active health facilities in the repository.
    /// </summary>
    Task<List<HealthFacility>> FindActiveFacilitiesAsync();

    /// <summary>
    /// Updates the health facility in the repository.
    /// </summary>
    Task UpdateAsync(HealthFacility facility);
}