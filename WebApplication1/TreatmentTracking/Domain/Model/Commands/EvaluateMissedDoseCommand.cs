namespace WebApplication1.TreatmentTracking.Domain.Model.Commands;

public record EvaluateMissedDoseCommand(
    string DailyDoseId
);