namespace WebApplication1.HealthyFacility.Domain.Models.Entities;

public class NurseAssignment
{
    public string Id { get; }
    public string FacilityId { get; }
    public string NurseId { get; }

    public NurseAssignment(string id, string facilityId, string nurseId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Nurse assignment id is required", nameof(id));

        if (string.IsNullOrWhiteSpace(facilityId))
            throw new ArgumentException("Facility id is required", nameof(facilityId));

        if (string.IsNullOrWhiteSpace(nurseId))
            throw new ArgumentException("Nurse id is required", nameof(nurseId));

        Id = id;
        FacilityId = facilityId;
        NurseId = nurseId;
    }

    // Constructor privado para serialización
    private NurseAssignment() { }

    public NurseAssignmentPrimitives ToPrimitives()
    {
        return new NurseAssignmentPrimitives
        {
            Id = Id,
            FacilityId = FacilityId,
            NurseId = NurseId
        };
    }

    public class NurseAssignmentPrimitives
    {
        public string Id { get; set; } = string.Empty;
        public string FacilityId { get; set; } = string.Empty;
        public string NurseId { get; set; } = string.Empty;
    }
}