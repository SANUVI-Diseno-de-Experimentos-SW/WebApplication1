namespace WebApplication1.TreatmentTracking.Domain.Model.Commands;

public record CompleteTreatmentCommand(
    string TreatmentId,
    string NurseId,
    string? Observation = null
);