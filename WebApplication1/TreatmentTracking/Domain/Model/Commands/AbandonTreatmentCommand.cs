namespace WebApplication1.TreatmentTracking.Domain.Model.Commands;

public record AbandonTreatmentCommand(
    string TreatmentId,
    string NurseId,
    string? Observation = null
);