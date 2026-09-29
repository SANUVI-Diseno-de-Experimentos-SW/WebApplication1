namespace WebApplication1.HealthyFacility.Domain.Models.Commands;


public record AssignNurseToFacilityCommand(
    string FacilityId,
    string NurseId
);