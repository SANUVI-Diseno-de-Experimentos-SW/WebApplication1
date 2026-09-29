namespace WebApplication1.TreatmentTracking.Domain.Model.Commands;

public record StartTreatmentCommand(
    string PatientId,
    string NurseId,
    string SupplementName,
    string Quantity,
    string DosingHours,
    int DurationDays
);