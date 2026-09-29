using WebApplication1.HealthyFacility.Domain.Models.Entities;
using WebApplication1.HealthyFacility.Infrastructure.Persitence.MongoDb.Models;

namespace WebApplication1.HealthyFacility.Infrastructure.Mappers;

public static class NurseAssignmentMapper
{
    public static NurseAssignment ToDomain(NurseAssignmentDocument document)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        return new NurseAssignment(
            document.NurseAssignmentId,
            document.FacilityId,
            document.NurseId
        );
    }

    public static NurseAssignmentDocument ToPersistence(NurseAssignment assignment)
    {
        if (assignment == null)
            throw new ArgumentNullException(nameof(assignment));

        var data = assignment.ToPrimitives();

        return new NurseAssignmentDocument
        {
            NurseAssignmentId = data.Id,      // ✅ Usar NurseAssignmentId
            FacilityId = data.FacilityId,
            NurseId = data.NurseId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static List<NurseAssignment> ToDomainList(IEnumerable<NurseAssignmentDocument> documents)
    {
        return documents.Select(ToDomain).ToList();
    }
}