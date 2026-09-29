namespace WebApplication1.TreatmentTracking.Domain.Model.Commands;

public record ConfirmDoseCommand(
    string PatientId,
    string MotherId
);