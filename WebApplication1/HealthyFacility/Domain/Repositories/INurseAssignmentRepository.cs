using WebApplication1.HealthyFacility.Domain.Models.Entities;

namespace WebApplication1.HealthyFacility.Domain.Repositories;


public interface INurseAssignmentRepository
{
    /// <summary>
    /// Saves the nurse assignment to the repository.
    /// </summary>
    Task<NurseAssignment> SaveAsync(NurseAssignment assignment);

    /// <summary>
    /// Finds all nurse assignments by facility id.
    /// </summary>
    Task<List<NurseAssignment>> FindByFacilityIdAsync(string facilityId);

    /// <summary>
    /// Finds all nurse assignments by nurse id.
    /// </summary>
    Task<NurseAssignment?> FindByNurseIdAsync(string nurseId);

    /// <summary>
    /// Finds active nurse assignment by facility id (should return only one).
    /// </summary>
    Task<NurseAssignment?> FindActiveByFacilityIdAsync(string facilityId);

    /// <summary>
    /// Finds active nurse assignment by nurse id (should return only one).
    /// </summary>
    Task<NurseAssignment?> FindActiveByNurseIdAsync(string nurseId);
}